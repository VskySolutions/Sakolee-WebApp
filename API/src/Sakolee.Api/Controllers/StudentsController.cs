using Sakolee.Api.Models.Profile;
using Sakolee.Api.Models.Students;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Auditing;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Abstractions.Security;
using Sakolee.Application.Abstractions.Tenancy;
using Sakolee.Application.Common;
using Sakolee.Domain.Entities;
using Sakolee.Domain.Enums;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sakolee.Api.Controllers;

/// <summary>
/// Student management: create, list, update, and delete students. Students carry no TenantId of
/// their own — ownership is resolved through PersonId (see <see cref="Student"/> remarks), so every
/// read/write here is checked against the caller's resolved tenant explicitly.
/// <para>
/// Creating a student is not a standalone write: it also mints the CRM <see cref="Person"/> master
/// record behind it (there is no existing one to link to, unlike Person or User creation) and a login
/// account for that person carrying the <see cref="Roles.Student"/> role — see <see cref="Create"/>.
/// </para>
/// </summary>
[ApiController]
[Authorize]
[Route("/api/admin/students")]
[Produces("application/json")]
[Tags("Students")]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
public sealed class StudentsController : ControllerBase
{
    private readonly IStudentRepository _students;
    private readonly IPersonRepository _persons;
    private readonly IAddressRepository _addresses;
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantContext _tenantContext;
    private readonly IActorAccessor _actorAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditTrailService _audit;

    public StudentsController(
        IStudentRepository students,
        IPersonRepository persons,
        IAddressRepository addresses,
        IUserRepository users,
        IRoleRepository roles,
        IPasswordHasher passwordHasher,
        ITenantContext tenantContext,
        IActorAccessor actorAccessor,
        IUnitOfWork unitOfWork,
        IAuditTrailService audit)
    {
        _students = students;
        _persons = persons;
        _addresses = addresses;
        _users = users;
        _roles = roles;
        _passwordHasher = passwordHasher;
        _tenantContext = tenantContext;
        _actorAccessor = actorAccessor;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    /// <summary>What the Students list may be ordered by. Name/email are not: they live on the linked
    /// Person, joined in after the query, and are not something this in-memory sort can reach.</summary>
    private static readonly SortMap<Student> Sorts = new SortMap<Student>("updatedOnUtc")
        .Add("active", s => s.Active, s => s.CreatedOnUtc)
        .Add("school", s => s.School, s => s.CreatedOnUtc)
        .Add("gradeLevel", s => s.GradeLevel, s => s.CreatedOnUtc)
        .Add("admissionDate", s => s.AdmissionDate)
        .Add("birthDate", s => s.BirthDate)
        .Add("createdOnUtc", s => s.CreatedOnUtc)
        .Add("updatedOnUtc", s => s.UpdatedOnUtc);

    [HttpPost]
    [RequirePermission(Permissions.StudentsWrite)]
    [ProducesResponseType<ApiResponse<StudentResponse>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request, CancellationToken cancellationToken)
    {
        // Creating the Person (and the login account below) needs a tenant to own them — there is no
        // request-level tenant picker here, unlike Person/User creation, because a student is always
        // created inside the caller's own active tenant.
        if (!_tenantContext.IsResolved)
        {
            return BadRequest(ApiResponseFactory.Error(
                ApiErrorCodes.ValidationFailed, "Validation failed.", "An active tenant is required to create a student."));
        }
        var tenantId = _tenantContext.TenantId;

        var email = request.Email.Trim();
        if (await _users.EmailExistsAsync(email, cancellationToken))
        {
            return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Email already in use.", email));
        }

        // Seeded on every startup (BootstrapSeeder) — see Roles.Student remarks.
        var studentRole = await _roles.GetByNameAsync(Roles.Student, cancellationToken)
            ?? throw new InvalidOperationException("The Student system role was not seeded.");

        var now = DateTime.UtcNow;
        var actorId = CurrentActorId();
        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        var fullName = string.Join(" ", new[] { firstName, lastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
        var userId = Guid.NewGuid();

        // 1. The CRM Person master record — there is no existing one to link to for a new student, so one
        // is minted here (mirrors the bootstrap admin and other Person-backed onboarding flows).
        var person = new Person
        {
            Id = Guid.NewGuid(),
            PersonCode = await GeneratePersonCodeAsync(cancellationToken),
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            DisplayName = fullName,
            Gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender.Trim(),
            DateOfBirth = request.BirthDate,
            PrimaryEmail = email,
            MobileNumber = request.CellPhone?.Trim(),
            EmergencyContactName = request.EmergencyContactName?.Trim(),
            EmergencyContactNumber = request.EmergencyContactNumber?.Trim(),
            IsActive = true,
            LastProfileUpdatedOn = now,
            SourceEntityType = EntityType.Student,
        };
        person.TenantMappings.Add(new TenantPersonMapping { Id = Guid.NewGuid(), TenantId = tenantId });
        if (request.Address is { } addressInput)
        {
            await UpsertAddressAsync(person, addressInput, cancellationToken);
        }
        await _persons.AddAsync(person, cancellationToken);

        // 2. The student row itself, linked to the Person above.
        var student = new Student
        {
            Id = Guid.NewGuid(),
            PersonId = person.Id,
            ParentId = request.ParentId,
            FamilyName = request.FamilyName?.Trim(),
            StudentNumber = request.StudentNumber?.Trim(),
            AdmissionDate = request.AdmissionDate,
            ClassId = request.ClassId,
            Active = true,
            FeeAmount = request.FeeAmount,
            FeeExpiryDate = request.FeeExpiryDate,
            FeeNote = request.FeeNote?.Trim(),
            FeeCategoryId = request.FeeCategoryId,
            BirthDate = request.BirthDate,
            CellPhone = request.CellPhone?.Trim(),
            School = request.School?.Trim(),
            GradeLevel = request.GradeLevel?.Trim(),
            Transportation = request.Transportation?.Trim(),
            TShirtSize = request.TShirtSize?.Trim(),
            Disabilities = request.Disabilities,
            SpecialNeeds = request.SpecialNeeds?.Trim(),
            Allergies = request.Allergies,
            Medications = request.Medications,
            PrimaryDoctor = request.PrimaryDoctor?.Trim(),
            HasImmunizations = request.HasImmunizations,
            ImmunizationNotes = request.ImmunizationNotes,
            SkillNotes = request.SkillNotes,
            TextOptIn = request.TextOptIn,
            MassEmailOptOut = request.MassEmailOptOut,
            HealthInsuranceCarrier = request.HealthInsuranceCarrier?.Trim(),
            DisabilitiesNotes = request.DisabilitiesNotes?.Trim(),
            AllergiesNotes = request.AllergiesNotes?.Trim(),
            AllowTextMessaging = request.AllowTextMessaging,
            CreatedOnUtc = now,
            CreatedById = actorId,
            UpdatedOnUtc = now,
            UpdatedById = actorId,
        };
        await _students.AddAsync(student, cancellationToken);

        // 3. The login account, promoted from the Person just created, carrying the Student role in the
        // active tenant — the same shape UsersController.Create builds for a promoted Person.
        var temporaryPassword = _passwordHasher.GenerateTemporaryPassword();
        var (hash, salt) = _passwordHasher.Hash(temporaryPassword);
        var user = new User
        {
            Id = userId,
            Email = email,
            DisplayName = fullName,
            PersonId = person.Id,
            PasswordHash = hash,
            Salt = salt,
            IsActive = true,
            MustChangePassword = true,
            TokenVersion = 1,
            CreatedDate = now,
        };
        await _users.AddAsync(user, cancellationToken);
        await _users.AddAssignmentAsync(new UserTenantRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            Role = RoleAssignment.MapLegacyRole(studentRole, null),
            RoleId = studentRole.Id,
        }, cancellationToken);

        await _audit.AddAsync(nameof(Student), student.Id.ToString(), "Created",
            details: student.StudentNumber, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponseFactory.Success(
                new StudentResponse(student.Id, person.Id, userId, firstName, lastName, student.Active, temporaryPassword),
                "Student created."));
    }

    [HttpGet]
    [RequirePermission(Permissions.StudentsRead)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] bool? active = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = true,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var all = await _students.ListAsync(_tenantContext.IsResolved ? _tenantContext.TenantId : null, cancellationToken);
        var persons = await LoadPersonsAsync(all, cancellationToken);
        IEnumerable<Student> filteredSet = all;

        if (active.HasValue)
        {
            filteredSet = filteredSet.Where(s => s.Active == active.Value);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            filteredSet = filteredSet.Where(s =>
            {
                var p = PersonFor(persons, s);
                return (p?.FirstName is { } fn && fn.Contains(term, StringComparison.OrdinalIgnoreCase))
                    || (p?.LastName is { } ln && ln.Contains(term, StringComparison.OrdinalIgnoreCase))
                    || (p?.PrimaryEmail is { } em && em.Contains(term, StringComparison.OrdinalIgnoreCase));
            });
        }

        var filtered = Sorts.Apply(filteredSet, sortBy, descending).ToList();
        var pageStudents = filtered.Skip((page - 1) * limit).Take(limit).ToList();
        var names = await ResolveActorNamesAsync(pageStudents.SelectMany(s => new[] { s.CreatedById, s.UpdatedById }), cancellationToken);
        var pageItems = pageStudents.Select(s => ToSummary(s, PersonFor(persons, s), names));

        return Ok(ApiResponseFactory.Paginated(pageItems, "Students retrieved.", page, limit, filtered.Count));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.StudentsRead)]
    [ProducesResponseType<ApiResponse<StudentSummary>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var student = await LoadOwnedAsync(id, cancellationToken);
        if (student is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Student not found."));
        }

        var person = student.PersonId is { } personId ? await _persons.GetByIdAsync(personId, cancellationToken) : null;
        var names = await ResolveActorNamesAsync(new[] { student.CreatedById, student.UpdatedById }, cancellationToken);
        return Ok(ApiResponseFactory.Success(ToSummary(student, person, names), "Student retrieved."));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.StudentsWrite)]
    [ProducesResponseType<ApiResponse<StudentResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentRequest request, CancellationToken cancellationToken)
    {
        var student = await LoadOwnedAsync(id, cancellationToken);
        if (student is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Student not found."));
        }

        var person = student.PersonId is { } personId ? await _persons.GetByIdAsync(personId, cancellationToken) : null;

        // Email lives on the login account first (it is what a student signs in with); the Person's copy
        // and the Student's own contact fields follow it.
        if (request.Email is { } newEmailRaw)
        {
            var newEmail = newEmailRaw.Trim();
            if (person?.UserId is { } linkedUserId)
            {
                var linkedUser = await _users.GetByIdAsync(linkedUserId, cancellationToken);
                if (linkedUser is not null && !string.Equals(newEmail, linkedUser.Email, StringComparison.OrdinalIgnoreCase))
                {
                    if (await _users.EmailExistsAsync(newEmail, cancellationToken))
                    {
                        return Conflict(ApiResponseFactory.Error(ApiErrorCodes.DuplicateIdentifier, "Email already in use.", newEmail));
                    }
                    linkedUser.Email = newEmail;
                    linkedUser.TokenVersion++; // email change invalidates sessions
                    _users.Update(linkedUser);
                }
            }
            if (person is not null)
            {
                person.PrimaryEmail = newEmail;
            }
        }

        if (person is not null)
        {
            if (request.FirstName is { } firstName)
            {
                person.FirstName = firstName.Trim();
            }
            if (request.LastName is { } lastName)
            {
                person.LastName = lastName.Trim();
            }
            if (request.FirstName is not null || request.LastName is not null)
            {
                person.DisplayName = person.FullName;
            }
            person.Gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender.Trim();
            person.EmergencyContactName = request.EmergencyContactName?.Trim();
            person.EmergencyContactNumber = request.EmergencyContactNumber?.Trim();
            if (request.Address is { } addressInput)
            {
                await UpsertAddressAsync(person, addressInput, cancellationToken);
            }
            person.LastProfileUpdatedOn = DateTime.UtcNow;
            _persons.Update(person);

            // Keep the linked user's display name in sync with the person's (PersonsController does the same).
            if (person.UserId is { } displayUserId)
            {
                var displayUser = await _users.GetByIdAsync(displayUserId, cancellationToken);
                if (displayUser is not null)
                {
                    displayUser.DisplayName = person.FullName;
                    _users.Update(displayUser);
                }
            }
        }

        student.ParentId = request.ParentId;
        student.FamilyName = request.FamilyName?.Trim();
        student.StudentNumber = request.StudentNumber?.Trim();
        student.AdmissionDate = request.AdmissionDate;
        student.ClassId = request.ClassId;
        student.Active = request.Active;
        student.FeeAmount = request.FeeAmount;
        student.FeeExpiryDate = request.FeeExpiryDate;
        student.FeeNote = request.FeeNote?.Trim();
        student.FeeCategoryId = request.FeeCategoryId;
        student.BirthDate = request.BirthDate;
        student.CellPhone = request.CellPhone?.Trim();
        student.School = request.School?.Trim();
        student.GradeLevel = request.GradeLevel?.Trim();
        student.Transportation = request.Transportation?.Trim();
        student.TShirtSize = request.TShirtSize?.Trim();
        student.Disabilities = request.Disabilities;
        student.SpecialNeeds = request.SpecialNeeds?.Trim();
        student.Allergies = request.Allergies;
        student.Medications = request.Medications;
        student.PrimaryDoctor = request.PrimaryDoctor?.Trim();
        student.HasImmunizations = request.HasImmunizations;
        student.ImmunizationNotes = request.ImmunizationNotes;
        student.SkillNotes = request.SkillNotes;
        student.TextOptIn = request.TextOptIn;
        student.MassEmailOptOut = request.MassEmailOptOut;
        student.HealthInsuranceCarrier = request.HealthInsuranceCarrier?.Trim();
        student.DisabilitiesNotes = request.DisabilitiesNotes?.Trim();
        student.AllergiesNotes = request.AllergiesNotes?.Trim();
        student.AllowTextMessaging = request.AllowTextMessaging;
        student.UpdatedOnUtc = DateTime.UtcNow;
        student.UpdatedById = CurrentActorId();
        _students.Update(student);

        await _audit.AddAsync(nameof(Student), student.Id.ToString(), "Updated", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponseFactory.Success(
            new StudentResponse(student.Id, student.PersonId, person?.UserId, person?.FirstName, person?.LastName, student.Active, null),
            "Student updated."));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.StudentsDelete)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var student = await LoadOwnedAsync(id, cancellationToken);
        if (student is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Student not found."));
        }

        student.Deleted = true;
        student.UpdatedOnUtc = DateTime.UtcNow;
        student.UpdatedById = CurrentActorId();
        _students.Update(student);
        await _audit.AddAsync(nameof(Student), student.Id.ToString(), "Deleted", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponseFactory.Success(new { studentId = id }, "Student deleted."));
    }

    // ---- helpers ----

    /// <summary>Loads a student and confirms it belongs to the caller's resolved tenant (via PersonId),
    /// returning null — the same as "not found" — for a foreign-tenant record.</summary>
    private async Task<Student?> LoadOwnedAsync(Guid id, CancellationToken cancellationToken)
    {
        var student = await _students.GetByIdAsync(id, cancellationToken);
        if (student is null)
        {
            return null;
        }
        if (_tenantContext.IsResolved && !await _students.IsOwnedByTenantAsync(student.PersonId, _tenantContext.TenantId, cancellationToken))
        {
            return null;
        }
        return student;
    }

    /// <summary>Batch-loads the Persons linked to a set of students, keyed by their id — the join Student
    /// itself can no longer do, now that identity fields live only on Person.</summary>
    private async Task<IReadOnlyDictionary<Guid, Person>> LoadPersonsAsync(IEnumerable<Student> students, CancellationToken cancellationToken)
    {
        var ids = students.Where(s => s.PersonId.HasValue).Select(s => s.PersonId!.Value).Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, Person>();
        }
        var persons = await _persons.GetByIdsAsync(ids, cancellationToken);
        return persons.ToDictionary(p => p.Id);
    }

    private static Person? PersonFor(IReadOnlyDictionary<Guid, Person> persons, Student student)
        => student.PersonId is { } id && persons.TryGetValue(id, out var person) ? person : null;

    /// <summary>Creates or updates the linked Person's Address — mirrors PersonsController's helper of
    /// the same name.</summary>
    private async Task UpsertAddressAsync(Person person, AddressInput input, CancellationToken cancellationToken)
    {
        var address = person.AddressId is { } addressId
            ? await _addresses.GetByIdAsync(addressId, cancellationToken)
            : null;

        var isNew = address is null;
        address ??= new Address { Id = Guid.NewGuid() };

        address.AddressType = Enum.TryParse<AddressType>(input.AddressType, ignoreCase: true, out var type) ? type : AddressType.Home;
        address.AddressLine1 = input.AddressLine1;
        address.AddressLine2 = input.AddressLine2;
        address.Landmark = input.Landmark;
        address.BuildingName = input.BuildingName;
        address.FloorNumber = input.FloorNumber;
        address.UnitNumber = input.UnitNumber;
        address.CountryCode = input.CountryCode;
        address.CountryName = input.CountryName;
        address.StateCode = input.StateCode;
        address.StateName = input.StateName;
        address.CityName = input.CityName;
        address.PostalCode = input.PostalCode;

        if (isNew)
        {
            await _addresses.AddAsync(address, cancellationToken);
            person.AddressId = address.Id;
            person.Address = address;
        }
        else
        {
            _addresses.Update(address);
        }
    }

    private Guid? CurrentActorId()
        => Guid.TryParse(_actorAccessor.GetCurrentActor(), out var id) ? id : null;

    /// <summary>Generates a unique business person code (e.g. PER-AB12CD34EF) — mirrors PersonsController's
    /// generator; Student creation mints a Person the same way Person creation does.</summary>
    private async Task<string> GeneratePersonCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var code = "PER-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
            if (!await _persons.PersonCodeExistsAsync(code, cancellationToken))
            {
                return code;
            }
        }

        return "PER-" + Guid.NewGuid().ToString("N").ToUpperInvariant();
    }

    private async Task<IReadOnlyDictionary<Guid, string>> ResolveActorNamesAsync(IEnumerable<Guid?> ids, CancellationToken cancellationToken)
        => await _users.GetFullNamesAsync(ids.Where(id => id.HasValue).Select(id => id!.Value), cancellationToken);

    private static string? NameOf(IReadOnlyDictionary<Guid, string> names, Guid? id)
        => id.HasValue && names.TryGetValue(id.Value, out var name) ? name : null;

    private static StudentSummary ToSummary(Student s, Person? person, IReadOnlyDictionary<Guid, string> names) => new(
        s.Id, s.PersonId, s.ParentId, person?.FirstName, person?.LastName, s.FamilyName, s.StudentNumber, s.AdmissionDate,
        s.ClassId, s.Active, s.FeeAmount, s.FeeExpiryDate, s.FeeNote, s.FeeCategoryId, person?.Gender, s.BirthDate,
        s.CellPhone, person?.PrimaryEmail, s.School, s.GradeLevel, s.Transportation, s.TShirtSize, s.Disabilities,
        s.SpecialNeeds, s.Allergies, s.Medications, s.PrimaryDoctor, s.HasImmunizations, s.ImmunizationNotes, s.SkillNotes,
        s.TextOptIn, s.MassEmailOptOut, s.HealthInsuranceCarrier, s.DisabilitiesNotes, s.AllergiesNotes, s.AllowTextMessaging,
        person?.EmergencyContactName, person?.EmergencyContactNumber,
        NameOf(names, s.CreatedById), s.CreatedOnUtc, NameOf(names, s.UpdatedById), s.UpdatedOnUtc);
}

using Sakolee.Api.Models.Classes;
using Sakolee.Api.Security;
using Sakolee.Application.Abstractions.Auditing;
using Sakolee.Application.Abstractions.Persistence;
using Sakolee.Application.Abstractions.Security;
using Sakolee.Application.Common;
using Sakolee.Domain.Entities;
using Sakolee.Shared.Contracts;
using Sakolee.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sakolee.Api.Controllers;

/// <summary>
/// Class management: create, list, update, and (soft-)delete classes. See the <see cref="Class"/>
/// entity remarks — classes carry no TenantId yet and are visible platform-wide.
/// </summary>
[ApiController]
[Authorize]
[Route("/api/admin/classes")]
[Produces("application/json")]
[Tags("Classes")]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
public sealed class ClassesController : ControllerBase
{
    private readonly IClassRepository _classes;
    private readonly IClassCategoryRepository _categories;
    private readonly ILocationRepository _locations;
    private readonly IClassSessionRepository _sessions;
    private readonly IUserRepository _users;
    private readonly IActorAccessor _actorAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditTrailService _audit;

    public ClassesController(
        IClassRepository classes,
        IClassCategoryRepository categories,
        ILocationRepository locations,
        IClassSessionRepository sessions,
        IUserRepository users,
        IActorAccessor actorAccessor,
        IUnitOfWork unitOfWork,
        IAuditTrailService audit)
    {
        _classes = classes;
        _categories = categories;
        _locations = locations;
        _sessions = sessions;
        _users = users;
        _actorAccessor = actorAccessor;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    /// <summary>What the Classes list may be ordered by.</summary>
    private static readonly SortMap<Class> Sorts = new SortMap<Class>("updatedOnUtc")
        .Add("className", c => c.ClassName, c => c.CreatedOnUtc)
        .Add("active", c => c.Active, c => c.CreatedOnUtc)
        .Add("startDate", c => c.StartDate)
        .Add("endDate", c => c.EndDate)
        .Add("createdOnUtc", c => c.CreatedOnUtc)
        .Add("updatedOnUtc", c => c.UpdatedOnUtc);

    [HttpPost]
    [RequirePermission(Permissions.ClassesWrite)]
    [ProducesResponseType<ApiResponse<ClassSummary>>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateClassRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var actorId = CurrentActorId();
        var entity = new Class
        {
            Id = Guid.NewGuid(),
            Active = true,
            CreatedOnUtc = now,
            CreatedById = actorId,
            UpdatedOnUtc = now,
            UpdatedById = actorId,
        };
        ApplyRequest(entity, request.LocationId, request.RoomId, request.SessionId, request.PrimaryInstructorId,
            request.Category1Id, request.Category2Id, request.Category3Id,
            request.ClassName, request.AdditionalInstructors, request.StartDate, request.EndDate,
            request.RegistrationOpenDate, request.ActiveDays, request.StartTime, request.EndTime, request.Duration,
            request.TuitionFee, request.BillingMethod, request.BillingCycle, request.RegistrationFee,
            request.Description, request.Gender, request.MinAge, request.MaxAge, request.MaxClassSize,
            request.MaxWaitlistSize, request.CutoffDate, request.PolicyGroups, request.VirtualClassUrl,
            request.LinkDisplayText, request.OnlineListings, request.OnlineRegistration, request.AllowWaitlistInRoll,
            request.AllowPortalEnrollment, request.AllowDropIns, request.ParentPortalSchedule, request.MakeupsInClass,
            request.AllowWaitlistEnrollment, request.AllowPortalDropRequests, request.DropInFee);

        await _classes.AddAsync(entity, cancellationToken);
        await _audit.AddAsync(nameof(Class), entity.Id.ToString(), "Created", details: entity.ClassName, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var names = await ResolveActorNamesAsync(new[] { entity.CreatedById, entity.UpdatedById }, cancellationToken);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponseFactory.Success(ToSummary(entity, names), "Class created."));
    }

    [HttpGet]
    [RequirePermission(Permissions.ClassesRead)]
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

        var all = await _classes.ListAsync(cancellationToken);
        IEnumerable<Class> filteredSet = all;

        if (active.HasValue)
        {
            filteredSet = filteredSet.Where(c => c.Active == active.Value);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            filteredSet = filteredSet.Where(c => c.ClassName is { } name && name.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var filtered = Sorts.Apply(filteredSet, sortBy, descending).ToList();
        var pageClasses = filtered.Skip((page - 1) * limit).Take(limit).ToList();
        var names = await ResolveActorNamesAsync(pageClasses.SelectMany(c => new[] { c.CreatedById, c.UpdatedById }), cancellationToken);
        var pageItems = pageClasses.Select(c => ToSummary(c, names));

        return Ok(ApiResponseFactory.Paginated(pageItems, "Classes retrieved.", page, limit, filtered.Count));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.ClassesRead)]
    [ProducesResponseType<ApiResponse<ClassSummary>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _classes.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Class not found."));
        }

        var names = await ResolveActorNamesAsync(new[] { entity.CreatedById, entity.UpdatedById }, cancellationToken);
        var categoryNames = await _categories.GetNamesAsync(
            new[] { entity.Category1Id, entity.Category2Id, entity.Category3Id }.Where(id => id.HasValue).Select(id => id!.Value),
            cancellationToken);
        string? CategoryName(Guid? id) => id is { } categoryId && categoryNames.TryGetValue(categoryId, out var name) ? name : null;
        var location = entity.LocationId is { } locationId ? await _locations.GetByIdAsync(locationId, cancellationToken) : null;
        var session = entity.SessionId is { } sessionId ? await _sessions.GetByIdAsync(sessionId, cancellationToken) : null;

        return Ok(ApiResponseFactory.Success(
            ToSummary(entity, names) with
            {
                Category1Name = CategoryName(entity.Category1Id),
                Category2Name = CategoryName(entity.Category2Id),
                Category3Name = CategoryName(entity.Category3Id),
                LocationName = location?.Name,
                SessionName = session?.Name,
            },
            "Class retrieved."));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.ClassesWrite)]
    [ProducesResponseType<ApiResponse<ClassSummary>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClassRequest request, CancellationToken cancellationToken)
    {
        var entity = await _classes.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Class not found."));
        }

        ApplyRequest(entity, request.LocationId, request.RoomId, request.SessionId, request.PrimaryInstructorId,
            request.Category1Id, request.Category2Id, request.Category3Id,
            request.ClassName, request.AdditionalInstructors, request.StartDate, request.EndDate,
            request.RegistrationOpenDate, request.ActiveDays, request.StartTime, request.EndTime, request.Duration,
            request.TuitionFee, request.BillingMethod, request.BillingCycle, request.RegistrationFee,
            request.Description, request.Gender, request.MinAge, request.MaxAge, request.MaxClassSize,
            request.MaxWaitlistSize, request.CutoffDate, request.PolicyGroups, request.VirtualClassUrl,
            request.LinkDisplayText, request.OnlineListings, request.OnlineRegistration, request.AllowWaitlistInRoll,
            request.AllowPortalEnrollment, request.AllowDropIns, request.ParentPortalSchedule, request.MakeupsInClass,
            request.AllowWaitlistEnrollment, request.AllowPortalDropRequests, request.DropInFee);
        entity.Active = request.Active;
        entity.UpdatedOnUtc = DateTime.UtcNow;
        entity.UpdatedById = CurrentActorId();
        _classes.Update(entity);

        await _audit.AddAsync(nameof(Class), entity.Id.ToString(), "Updated", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var names = await ResolveActorNamesAsync(new[] { entity.CreatedById, entity.UpdatedById }, cancellationToken);
        return Ok(ApiResponseFactory.Success(ToSummary(entity, names), "Class updated."));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.ClassesDelete)]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _classes.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return NotFound(ApiResponseFactory.NotFound("Class not found."));
        }

        entity.Deleted = true;
        entity.UpdatedOnUtc = DateTime.UtcNow;
        entity.UpdatedById = CurrentActorId();
        _classes.Update(entity);
        await _audit.AddAsync(nameof(Class), entity.Id.ToString(), "Deleted", cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponseFactory.Success(new { classId = id }, "Class deleted."));
    }

    // ---- helpers ----

    private static void ApplyRequest(
        Class entity, Guid? locationId, Guid? roomId, Guid? sessionId, Guid? primaryInstructorId,
        Guid? category1Id, Guid? category2Id, Guid? category3Id,
        string className, string? additionalInstructors, DateTime? startDate, DateTime? endDate,
        DateTime? registrationOpenDate, string? activeDays, string? startTime, string? endTime, string? duration,
        decimal? tuitionFee, string? billingMethod, string? billingCycle, bool? registrationFee,
        string? description, string? gender, int? minAge, int? maxAge, int? maxClassSize, int? maxWaitlistSize,
        DateTime? cutoffDate, string? policyGroups, string? virtualClassUrl, string? linkDisplayText,
        bool onlineListings, bool onlineRegistration, bool allowWaitlistInRoll, bool allowPortalEnrollment,
        bool allowDropIns, bool parentPortalSchedule, bool makeupsInClass, bool allowWaitlistEnrollment,
        bool allowPortalDropRequests, bool dropInFee)
    {
        entity.LocationId = locationId;
        entity.RoomId = roomId;
        entity.SessionId = sessionId;
        entity.PrimaryInstructorId = primaryInstructorId;
        entity.Category1Id = category1Id;
        entity.Category2Id = category2Id;
        entity.Category3Id = category3Id;
        entity.ClassName = className.Trim();
        entity.AdditionalInstructors = additionalInstructors?.Trim();
        entity.StartDate = startDate;
        entity.EndDate = endDate;
        entity.RegistrationOpenDate = registrationOpenDate;
        entity.ActiveDays = activeDays?.Trim();
        entity.StartTime = startTime?.Trim();
        entity.EndTime = endTime?.Trim();
        entity.Duration = duration?.Trim();
        entity.TuitionFee = tuitionFee;
        entity.BillingMethod = billingMethod?.Trim();
        entity.BillingCycle = billingCycle?.Trim();
        entity.RegistrationFee = registrationFee;
        entity.Description = description;
        entity.Gender = gender?.Trim();
        entity.MinAge = minAge;
        entity.MaxAge = maxAge;
        entity.MaxClassSize = maxClassSize;
        entity.MaxWaitlistSize = maxWaitlistSize;
        entity.CutoffDate = cutoffDate;
        entity.PolicyGroups = policyGroups?.Trim();
        entity.VirtualClassUrl = virtualClassUrl?.Trim();
        entity.LinkDisplayText = linkDisplayText?.Trim();
        entity.OnlineListings = onlineListings;
        entity.OnlineRegistration = onlineRegistration;
        entity.AllowWaitlistInRoll = allowWaitlistInRoll;
        entity.AllowPortalEnrollment = allowPortalEnrollment;
        entity.AllowDropIns = allowDropIns;
        entity.ParentPortalSchedule = parentPortalSchedule;
        entity.MakeupsInClass = makeupsInClass;
        entity.AllowWaitlistEnrollment = allowWaitlistEnrollment;
        entity.AllowPortalDropRequests = allowPortalDropRequests;
        entity.DropInFee = dropInFee;
    }

    private Guid? CurrentActorId()
        => Guid.TryParse(_actorAccessor.GetCurrentActor(), out var id) ? id : null;

    private async Task<IReadOnlyDictionary<Guid, string>> ResolveActorNamesAsync(IEnumerable<Guid?> ids, CancellationToken cancellationToken)
        => await _users.GetFullNamesAsync(ids.Where(id => id.HasValue).Select(id => id!.Value), cancellationToken);

    private static string? NameOf(IReadOnlyDictionary<Guid, string> names, Guid? id)
        => id.HasValue && names.TryGetValue(id.Value, out var name) ? name : null;

    private static ClassSummary ToSummary(Class c, IReadOnlyDictionary<Guid, string> names) => new(
        c.Id, c.LocationId, c.RoomId, c.SessionId, c.PrimaryInstructorId, c.Category1Id, c.Category2Id, c.Category3Id,
        c.ClassName, c.AdditionalInstructors,
        c.StartDate, c.EndDate, c.RegistrationOpenDate, c.ActiveDays, c.StartTime, c.EndTime, c.Duration,
        c.TuitionFee, c.BillingMethod, c.BillingCycle, c.RegistrationFee, c.Description, c.Gender, c.MinAge,
        c.MaxAge, c.MaxClassSize, c.MaxWaitlistSize, c.CutoffDate, c.PolicyGroups, c.VirtualClassUrl,
        c.LinkDisplayText, c.OnlineListings, c.OnlineRegistration, c.AllowWaitlistInRoll, c.AllowPortalEnrollment,
        c.AllowDropIns, c.ParentPortalSchedule, c.MakeupsInClass, c.AllowWaitlistEnrollment,
        c.AllowPortalDropRequests, c.DropInFee, c.Active,
        NameOf(names, c.CreatedById), c.CreatedOnUtc, NameOf(names, c.UpdatedById), c.UpdatedOnUtc);
}

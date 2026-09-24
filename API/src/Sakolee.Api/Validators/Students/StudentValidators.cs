using FluentValidation;
using Sakolee.Api.Models.Students;

namespace Sakolee.Api.Validators.Students;

public sealed class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        // Required — a login account is created alongside every student, and an account needs an email.
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.FamilyName).MaximumLength(50).When(x => x.FamilyName is not null);
        RuleFor(x => x.StudentNumber).MaximumLength(100).When(x => x.StudentNumber is not null);
        RuleFor(x => x.CellPhone).MaximumLength(50).When(x => x.CellPhone is not null);
        RuleFor(x => x.School).MaximumLength(100).When(x => x.School is not null);
        RuleFor(x => x.GradeLevel).MaximumLength(50).When(x => x.GradeLevel is not null);
        RuleFor(x => x.Transportation).MaximumLength(100).When(x => x.Transportation is not null);
        RuleFor(x => x.TShirtSize).MaximumLength(10).When(x => x.TShirtSize is not null);
        RuleFor(x => x.SpecialNeeds).MaximumLength(50).When(x => x.SpecialNeeds is not null);
        RuleFor(x => x.PrimaryDoctor).MaximumLength(50).When(x => x.PrimaryDoctor is not null);
        RuleFor(x => x.FeeNote).MaximumLength(300).When(x => x.FeeNote is not null);
        RuleFor(x => x.FeeAmount).GreaterThanOrEqualTo(0).When(x => x.FeeAmount.HasValue);
        RuleFor(x => x.Gender).MaximumLength(32).When(x => x.Gender is not null);
        RuleFor(x => x.HasImmunizations).MaximumLength(20).When(x => x.HasImmunizations is not null);
        RuleFor(x => x.HealthInsuranceCarrier).MaximumLength(100).When(x => x.HealthInsuranceCarrier is not null);
        RuleFor(x => x.DisabilitiesNotes).MaximumLength(500).When(x => x.DisabilitiesNotes is not null);
        RuleFor(x => x.AllergiesNotes).MaximumLength(500).When(x => x.AllergiesNotes is not null);
    }
}

public sealed class CreateStudentsBulkRequestValidator : AbstractValidator<CreateStudentsBulkRequest>
{
    public CreateStudentsBulkRequestValidator()
    {
        RuleFor(x => x.Students).NotEmpty().WithMessage("At least one student is required.");
        RuleForEach(x => x.Students).SetValidator(new CreateStudentRequestValidator());
        // Cross-item check: the batch's own emails must be distinct from each other, on top of each
        // one individually being checked against already-registered emails in StudentsController.
        RuleFor(x => x.Students)
            .Must(students => students
                .Select(s => s.Email.Trim().ToLowerInvariant())
                .Distinct().Count() == students.Count)
            .WithMessage("Every student in the batch must have a distinct email.")
            .When(x => x.Students.Count > 1);
    }
}

public sealed class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequest>
{
    public UpdateStudentRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).When(x => x.FirstName is not null);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).When(x => x.LastName is not null);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256).When(x => x.Email is not null);
        RuleFor(x => x.FamilyName).MaximumLength(50).When(x => x.FamilyName is not null);
        RuleFor(x => x.StudentNumber).MaximumLength(100).When(x => x.StudentNumber is not null);
        RuleFor(x => x.CellPhone).MaximumLength(50).When(x => x.CellPhone is not null);
        RuleFor(x => x.School).MaximumLength(100).When(x => x.School is not null);
        RuleFor(x => x.GradeLevel).MaximumLength(50).When(x => x.GradeLevel is not null);
        RuleFor(x => x.Transportation).MaximumLength(100).When(x => x.Transportation is not null);
        RuleFor(x => x.TShirtSize).MaximumLength(10).When(x => x.TShirtSize is not null);
        RuleFor(x => x.SpecialNeeds).MaximumLength(50).When(x => x.SpecialNeeds is not null);
        RuleFor(x => x.PrimaryDoctor).MaximumLength(50).When(x => x.PrimaryDoctor is not null);
        RuleFor(x => x.FeeNote).MaximumLength(300).When(x => x.FeeNote is not null);
        RuleFor(x => x.FeeAmount).GreaterThanOrEqualTo(0).When(x => x.FeeAmount.HasValue);
        RuleFor(x => x.Gender).MaximumLength(32).When(x => x.Gender is not null);
        RuleFor(x => x.HasImmunizations).MaximumLength(20).When(x => x.HasImmunizations is not null);
        RuleFor(x => x.HealthInsuranceCarrier).MaximumLength(100).When(x => x.HealthInsuranceCarrier is not null);
        RuleFor(x => x.DisabilitiesNotes).MaximumLength(500).When(x => x.DisabilitiesNotes is not null);
        RuleFor(x => x.AllergiesNotes).MaximumLength(500).When(x => x.AllergiesNotes is not null);
    }
}

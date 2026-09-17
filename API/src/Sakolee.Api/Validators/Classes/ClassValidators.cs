using FluentValidation;
using Sakolee.Api.Models.Classes;

namespace Sakolee.Api.Validators.Classes;

public sealed class CreateClassRequestValidator : AbstractValidator<CreateClassRequest>
{
    public CreateClassRequestValidator()
    {
        RuleFor(x => x.ClassName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AdditionalInstructors).MaximumLength(50).When(x => x.AdditionalInstructors is not null);
        RuleFor(x => x.ActiveDays).MaximumLength(200).When(x => x.ActiveDays is not null);
        RuleFor(x => x.StartTime).MaximumLength(20).When(x => x.StartTime is not null);
        RuleFor(x => x.EndTime).MaximumLength(20).When(x => x.EndTime is not null);
        RuleFor(x => x.Duration).MaximumLength(50).When(x => x.Duration is not null);
        RuleFor(x => x.TuitionFee).GreaterThanOrEqualTo(0).When(x => x.TuitionFee.HasValue);
        RuleFor(x => x.BillingMethod).MaximumLength(20).When(x => x.BillingMethod is not null);
        RuleFor(x => x.BillingCycle).MaximumLength(50).When(x => x.BillingCycle is not null);
        RuleFor(x => x.Gender).MaximumLength(30).When(x => x.Gender is not null);
        RuleFor(x => x.MinAge).GreaterThanOrEqualTo(0).When(x => x.MinAge.HasValue);
        RuleFor(x => x.MaxAge).GreaterThanOrEqualTo(0).When(x => x.MaxAge.HasValue);
        RuleFor(x => x.MaxClassSize).GreaterThanOrEqualTo(0).When(x => x.MaxClassSize.HasValue);
        RuleFor(x => x.MaxWaitlistSize).GreaterThanOrEqualTo(0).When(x => x.MaxWaitlistSize.HasValue);
        RuleFor(x => x.PolicyGroups).MaximumLength(50).When(x => x.PolicyGroups is not null);
        RuleFor(x => x.VirtualClassUrl).MaximumLength(300).When(x => x.VirtualClassUrl is not null);
        RuleFor(x => x.LinkDisplayText).MaximumLength(300).When(x => x.LinkDisplayText is not null);
    }
}

public sealed class UpdateClassRequestValidator : AbstractValidator<UpdateClassRequest>
{
    public UpdateClassRequestValidator()
    {
        RuleFor(x => x.ClassName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AdditionalInstructors).MaximumLength(50).When(x => x.AdditionalInstructors is not null);
        RuleFor(x => x.ActiveDays).MaximumLength(200).When(x => x.ActiveDays is not null);
        RuleFor(x => x.StartTime).MaximumLength(20).When(x => x.StartTime is not null);
        RuleFor(x => x.EndTime).MaximumLength(20).When(x => x.EndTime is not null);
        RuleFor(x => x.Duration).MaximumLength(50).When(x => x.Duration is not null);
        RuleFor(x => x.TuitionFee).GreaterThanOrEqualTo(0).When(x => x.TuitionFee.HasValue);
        RuleFor(x => x.BillingMethod).MaximumLength(20).When(x => x.BillingMethod is not null);
        RuleFor(x => x.BillingCycle).MaximumLength(50).When(x => x.BillingCycle is not null);
        RuleFor(x => x.Gender).MaximumLength(30).When(x => x.Gender is not null);
        RuleFor(x => x.MinAge).GreaterThanOrEqualTo(0).When(x => x.MinAge.HasValue);
        RuleFor(x => x.MaxAge).GreaterThanOrEqualTo(0).When(x => x.MaxAge.HasValue);
        RuleFor(x => x.MaxClassSize).GreaterThanOrEqualTo(0).When(x => x.MaxClassSize.HasValue);
        RuleFor(x => x.MaxWaitlistSize).GreaterThanOrEqualTo(0).When(x => x.MaxWaitlistSize.HasValue);
        RuleFor(x => x.PolicyGroups).MaximumLength(50).When(x => x.PolicyGroups is not null);
        RuleFor(x => x.VirtualClassUrl).MaximumLength(300).When(x => x.VirtualClassUrl is not null);
        RuleFor(x => x.LinkDisplayText).MaximumLength(300).When(x => x.LinkDisplayText is not null);
    }
}

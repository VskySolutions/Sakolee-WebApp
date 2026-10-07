using FluentValidation;
using Sakolee.Api.Models.Families;
using Sakolee.Api.Models.Profile;

namespace Sakolee.Api.Validators.Families;

public sealed class FamilyContactInputValidator : AbstractValidator<FamilyContactInput>
{
    public FamilyContactInputValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(600);
        RuleFor(x => x.Phone).MaximumLength(60).When(x => x.Phone is not null);
        RuleFor(x => x.Relation).MaximumLength(50).When(x => x.Relation is not null);
    }
}

/// <summary>
/// Length limits for a family's household address, matching the <c>Addresses</c> columns (see
/// <c>AddressConfiguration</c>). Every field is optional.
/// </summary>
public sealed class FamilyAddressInputValidator : AbstractValidator<AddressInput>
{
    public FamilyAddressInputValidator()
    {
        RuleFor(x => x.AddressType).MaximumLength(20).When(x => x.AddressType is not null);
        RuleFor(x => x.AddressLine1).MaximumLength(256).When(x => x.AddressLine1 is not null);
        RuleFor(x => x.AddressLine2).MaximumLength(256).When(x => x.AddressLine2 is not null);
        RuleFor(x => x.Landmark).MaximumLength(128).When(x => x.Landmark is not null);
        RuleFor(x => x.BuildingName).MaximumLength(128).When(x => x.BuildingName is not null);
        RuleFor(x => x.FloorNumber).MaximumLength(32).When(x => x.FloorNumber is not null);
        RuleFor(x => x.UnitNumber).MaximumLength(32).When(x => x.UnitNumber is not null);
        RuleFor(x => x.CountryCode).MaximumLength(3).When(x => x.CountryCode is not null);
        RuleFor(x => x.CountryName).MaximumLength(100).When(x => x.CountryName is not null);
        RuleFor(x => x.StateCode).MaximumLength(10).When(x => x.StateCode is not null);
        RuleFor(x => x.StateName).MaximumLength(100).When(x => x.StateName is not null);
        RuleFor(x => x.CityName).MaximumLength(100).When(x => x.CityName is not null);
        RuleFor(x => x.PostalCode).MaximumLength(20).When(x => x.PostalCode is not null);
    }
}

public sealed class CreateFamilyRequestValidator : AbstractValidator<CreateFamilyRequest>
{
    public CreateFamilyRequestValidator()
    {
        RuleFor(x => x.FamilyName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
        RuleFor(x => x.Relation).MaximumLength(100).When(x => x.Relation is not null);
        RuleFor(x => x.HomePhone).MaximumLength(100).When(x => x.HomePhone is not null);
        RuleFor(x => x.WorkPhone).MaximumLength(100).When(x => x.WorkPhone is not null);
        RuleFor(x => x.CellPhone).MaximumLength(100).When(x => x.CellPhone is not null);
        RuleFor(x => x.Fax).MaximumLength(100).When(x => x.Fax is not null);
        RuleFor(x => x.OtherPhone).MaximumLength(100).When(x => x.OtherPhone is not null);
        RuleFor(x => x.Source).MaximumLength(20).When(x => x.Source is not null);
        RuleFor(x => x.ReferralName).MaximumLength(50).When(x => x.ReferralName is not null);
        RuleFor(x => x.EmergencyContactPerson).MaximumLength(150).When(x => x.EmergencyContactPerson is not null);
        RuleFor(x => x.EmergencyPhone).MaximumLength(30).When(x => x.EmergencyPhone is not null);
        RuleFor(x => x.HealthInsuranceCarrier).MaximumLength(150).When(x => x.HealthInsuranceCarrier is not null);

        RuleFor(x => x.Address!).SetValidator(new FamilyAddressInputValidator()).When(x => x.Address is not null);
        RuleFor(x => x.SecondaryContact!).SetValidator(new FamilyContactInputValidator()).When(x => x.SecondaryContact is not null);
    }
}

public sealed class UpdateFamilyRequestValidator : AbstractValidator<UpdateFamilyRequest>
{
    public UpdateFamilyRequestValidator()
    {
        RuleFor(x => x.FamilyName).NotEmpty().MaximumLength(100).When(x => x.FamilyName is not null);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).When(x => x.FirstName is not null);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).When(x => x.LastName is not null);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100).When(x => x.Email is not null);
        RuleFor(x => x.Relation).MaximumLength(100).When(x => x.Relation is not null);
        RuleFor(x => x.HomePhone).MaximumLength(100).When(x => x.HomePhone is not null);
        RuleFor(x => x.WorkPhone).MaximumLength(100).When(x => x.WorkPhone is not null);
        RuleFor(x => x.CellPhone).MaximumLength(100).When(x => x.CellPhone is not null);
        RuleFor(x => x.Fax).MaximumLength(100).When(x => x.Fax is not null);
        RuleFor(x => x.OtherPhone).MaximumLength(100).When(x => x.OtherPhone is not null);
        RuleFor(x => x.Source).MaximumLength(20).When(x => x.Source is not null);
        RuleFor(x => x.ReferralName).MaximumLength(50).When(x => x.ReferralName is not null);
        RuleFor(x => x.EmergencyContactPerson).MaximumLength(150).When(x => x.EmergencyContactPerson is not null);
        RuleFor(x => x.EmergencyPhone).MaximumLength(30).When(x => x.EmergencyPhone is not null);
        RuleFor(x => x.HealthInsuranceCarrier).MaximumLength(150).When(x => x.HealthInsuranceCarrier is not null);

        RuleFor(x => x.Address!).SetValidator(new FamilyAddressInputValidator()).When(x => x.Address is not null);
        RuleFor(x => x.SecondaryContact!).SetValidator(new FamilyContactInputValidator()).When(x => x.SecondaryContact is not null);
    }
}

using FluentValidation;
using Sakolee.Api.Models.Tenants;
using Sakolee.Api.Validators;

namespace Sakolee.Api.Validators.Tenants;

public sealed class CreateTenantRequestValidator : AbstractValidator<CreateTenantRequest>
{
    public CreateTenantRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Identifier)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Identifier must be a URL-safe slug (lowercase letters, digits, hyphens).");

        // The default Administrator minted alongside the tenant (WO-61).
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).MustBeAPersonName("First name");
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).MustBeAPersonName("Last name");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.").EmailAddress().MaximumLength(256);
    }
}

public sealed class UpdateTenantRequestValidator : AbstractValidator<UpdateTenantRequest>
{
    public UpdateTenantRequestValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
}

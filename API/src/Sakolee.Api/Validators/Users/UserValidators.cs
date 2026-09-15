using FluentValidation;
using Sakolee.Api.Models.Users;
using Sakolee.Api.Validators;

namespace Sakolee.Api.Validators.Users;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        // The new Person master record is minted from these (WO-61).
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).MustBeAPersonName("First name");
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).MustBeAPersonName("Last name");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.").EmailAddress().MaximumLength(256);
        // At least one role (multi-role roleIds, the legacy single roleId, or the legacy role name).
        RuleFor(x => x)
            .Must(x => x.RoleIds is { Count: > 0 } || x.RoleId is not null || !string.IsNullOrWhiteSpace(x.Role))
            .WithMessage("At least one role is required (roleIds, roleId, or role).");
    }
}

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.DisplayName).MaximumLength(200).When(x => x.DisplayName is not null);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(256).When(x => x.Email is not null);
        RuleFor(x => x).Must(x => x.DisplayName is not null || x.Email is not null)
            .WithMessage("At least one of displayName or email must be provided.");
    }
}

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
    }
}

public sealed class SetUserDepartmentRequestValidator : AbstractValidator<SetUserDepartmentRequest>
{
    public SetUserDepartmentRequestValidator()
    {
        // A null/empty department clears the placement; anything supplied must fit the stored code column
        // (and is checked against the tenant's User.Department list by the controller).
        RuleFor(x => x.Department).MaximumLength(64).When(x => !string.IsNullOrWhiteSpace(x.Department));
    }
}

public sealed class AssignTenantRoleRequestValidator : AbstractValidator<AssignTenantRoleRequest>
{
    public AssignTenantRoleRequestValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        // At least one role id is required — the request reconciles the tenant's full role set.
        RuleFor(x => x)
            .Must(x => x.RoleIds is { Count: > 0 } || x.RoleId is not null || !string.IsNullOrWhiteSpace(x.Role))
            .WithMessage("At least one role is required (roleIds, roleId, or role).");
    }
}

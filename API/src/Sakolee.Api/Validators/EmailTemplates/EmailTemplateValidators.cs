using FluentValidation;
using Sakolee.Api.Models.EmailTemplates;

namespace Sakolee.Api.Validators.EmailTemplates;

public sealed class SaveEmailTemplateRequestValidator : AbstractValidator<SaveEmailTemplateRequest>
{
    public SaveEmailTemplateRequestValidator()
    {
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Body).NotEmpty();
    }
}

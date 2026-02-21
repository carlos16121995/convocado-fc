using ConvocadoFc.WebApi.Modules.Teams.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Validators.Teams;

public sealed class UpdateTeamRuleRequestValidator : AbstractValidator<UpdateTeamRuleRequest>
{
    public UpdateTeamRuleRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(request => request.Description)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.Description));

        RuleFor(request => request.Scope)
            .MaximumLength(80)
            .When(request => !string.IsNullOrWhiteSpace(request.Scope));

        RuleFor(request => request.Target)
            .MaximumLength(120)
            .When(request => !string.IsNullOrWhiteSpace(request.Target));
    }
}

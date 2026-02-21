using ConvocadoFc.WebApi.Modules.Teams.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Validators.Teams;

public sealed class AddTeamRuleParameterRequestValidator : AbstractValidator<AddTeamRuleParameterRequest>
{
    public AddTeamRuleParameterRequestValidator()
    {
        RuleFor(request => request.Key)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(request => request.Value)
            .NotEmpty()
            .MaximumLength(1200);

        RuleFor(request => request.ValueType)
            .MaximumLength(40)
            .When(request => !string.IsNullOrWhiteSpace(request.ValueType));

        RuleFor(request => request.Unit)
            .MaximumLength(40)
            .When(request => !string.IsNullOrWhiteSpace(request.Unit));

        RuleFor(request => request.Description)
            .MaximumLength(300)
            .When(request => !string.IsNullOrWhiteSpace(request.Description));
    }
}

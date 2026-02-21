using ConvocadoFc.WebApi.Modules.Teams.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Validators.Teams;

public sealed class CreateTeamRequestValidator : AbstractValidator<CreateTeamRequest>
{
    public CreateTeamRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(request => request.HomeFieldName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(request => request.HomeFieldAddress)
            .MaximumLength(300);

        RuleFor(request => request.CrestUrl)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.CrestUrl));
    }
}

using ConvocadoFc.WebApi.Modules.Teams.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Validators.Teams;

public sealed class UpdateMyProfileRequestValidator : AbstractValidator<UpdateMyProfileRequest>
{
    public UpdateMyProfileRequestValidator()
    {
        RuleFor(request => request)
            .Must(request => request.PrimaryPosition.HasValue
                             || request.SecondaryPosition.HasValue
                             || request.TertiaryPosition.HasValue
                             || request.CopyFromTeamId.HasValue
                             || request.IsOnHiatus.HasValue
                             || request.HiatusEndsAt.HasValue)
            .WithMessage("Informe ao menos uma posição ou time de origem.");
    }
}

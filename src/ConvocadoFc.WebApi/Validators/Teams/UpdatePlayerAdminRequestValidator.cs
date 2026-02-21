using ConvocadoFc.WebApi.Modules.Teams.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Validators.Teams;

public sealed class UpdatePlayerAdminRequestValidator : AbstractValidator<UpdatePlayerAdminRequest>
{
    public UpdatePlayerAdminRequestValidator()
    {
        RuleFor(request => request)
            .Must(request => request.IsFeeExempt.HasValue)
            .WithMessage("Informe ao menos uma alteração.");
    }
}

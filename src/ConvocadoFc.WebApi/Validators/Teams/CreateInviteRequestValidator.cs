using ConvocadoFc.Domain.Models.Modules.Teams;
using ConvocadoFc.WebApi.Modules.Teams.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Validators.Teams;

public sealed class CreateInviteRequestValidator : AbstractValidator<CreateInviteRequest>
{
    public CreateInviteRequestValidator()
    {
        RuleFor(request => request.TargetEmail)
            .MaximumLength(320)
            .When(request => !string.IsNullOrWhiteSpace(request.TargetEmail));

        RuleFor(request => request.Message)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.Message));

        RuleFor(request => request.MaxUses)
            .GreaterThan(0)
            .When(request => request.MaxUses.HasValue);

        RuleFor(request => request)
            .Must(request => request.Channel != ETeamInviteChannel.Email
                             || request.TargetUserId.HasValue
                             || !string.IsNullOrWhiteSpace(request.TargetEmail))
            .WithMessage("Informe o usuário ou e-mail do convite.");
    }
}

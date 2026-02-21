using ConvocadoFc.WebApi.Modules.Subscriptions.Models;

using FluentValidation;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Validators;

public sealed class ChangeSubscriptionRequestValidator : AbstractValidator<ChangeSubscriptionRequest>
{
    public ChangeSubscriptionRequestValidator()
    {
        RuleFor(x => x).Custom((request, context) =>
        {
            if (!request.PlanId.HasValue && !request.EndsAt.HasValue && !request.AutoRenew.HasValue && !request.Status.HasValue)
            {
                context.AddFailure("Changes", "Informe ao menos uma alteração para a assinatura.");
            }

            if (request.EndsAt.HasValue && request.EndsAt.Value < DateTimeOffset.UtcNow.AddMinutes(-1))
            {
                context.AddFailure(nameof(request.EndsAt), "EndsAt deve ser uma data futura.");
            }
        });
    }
}

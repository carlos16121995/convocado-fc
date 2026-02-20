using ConvocadoFc.WebApi.Modules.Subscriptions.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Validators;

public sealed class RemoveSubscriptionRequestValidator : AbstractValidator<RemoveSubscriptionRequest>
{
    public RemoveSubscriptionRequestValidator()
    {
        RuleFor(x => x.SubscriptionId).NotEmpty();
    }
}

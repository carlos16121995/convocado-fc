using ConvocadoFc.WebApi.Modules.Subscriptions.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Validators;

public sealed class AssignSubscriptionRequestValidator : AbstractValidator<AssignSubscriptionRequest>
{
    public AssignSubscriptionRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PlanId).NotEmpty();

        RuleFor(x => x).Custom((request, context) =>
        {
            if (request.StartsAt.HasValue && request.EndsAt.HasValue && request.EndsAt < request.StartsAt)
            {
                context.AddFailure(nameof(request.EndsAt), "EndsAt deve ser maior que StartsAt.");
            }
        });
    }
}

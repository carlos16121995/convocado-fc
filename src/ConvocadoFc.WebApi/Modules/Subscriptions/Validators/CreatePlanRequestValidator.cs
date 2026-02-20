using ConvocadoFc.WebApi.Modules.Subscriptions.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Validators;

public sealed class CreatePlanRequestValidator : AbstractValidator<CreatePlanRequest>
{
    public CreatePlanRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(10);

        RuleFor(x => x).Custom((request, context) =>
        {
            if (!request.IsCustomPricing)
            {
                if (!request.MaxTeams.HasValue || request.MaxTeams <= 0)
                {
                    context.AddFailure(nameof(request.MaxTeams), "MaxTeams é obrigatório e deve ser maior que zero.");
                }

                if (!request.MaxMembersPerTeam.HasValue || request.MaxMembersPerTeam <= 0)
                {
                    context.AddFailure(nameof(request.MaxMembersPerTeam), "MaxMembersPerTeam é obrigatório e deve ser maior que zero.");
                }
            }
        });
    }
}

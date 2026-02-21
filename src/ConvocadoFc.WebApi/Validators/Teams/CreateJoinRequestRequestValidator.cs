using ConvocadoFc.WebApi.Modules.Teams.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Validators.Teams;

public sealed class CreateJoinRequestRequestValidator : AbstractValidator<CreateJoinRequestRequest>
{
    public CreateJoinRequestRequestValidator()
    {
        RuleFor(request => request.Message)
            .MaximumLength(500)
            .When(request => !string.IsNullOrWhiteSpace(request.Message));
    }
}

using ConvocadoFc.WebApi.Modules.Teams.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Validators.Teams;

public sealed class AssignModeratorRequestValidator : AbstractValidator<AssignModeratorRequest>
{
    public AssignModeratorRequestValidator()
    {
        RuleFor(request => request.UserId)
            .NotEmpty();
    }
}

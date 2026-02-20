using ConvocadoFc.WebApi.Modules.Users.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Modules.Users.Validators;

public sealed class RemoveRoleRequestValidator : AbstractValidator<RemoveRoleRequest>
{
    public RemoveRoleRequestValidator()
    {
        RuleFor(request => request.UserId)
            .NotEmpty();

        RuleFor(request => request.Role)
            .NotEmpty();
    }
}

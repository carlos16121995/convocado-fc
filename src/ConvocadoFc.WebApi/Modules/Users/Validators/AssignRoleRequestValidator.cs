using ConvocadoFc.WebApi.Modules.Users.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Modules.Users.Validators;

public sealed class AssignRoleRequestValidator : AbstractValidator<AssignRoleRequest>
{
    public AssignRoleRequestValidator()
    {
        RuleFor(request => request.UserId)
            .NotEmpty();

        RuleFor(request => request.Role)
            .NotEmpty();
    }
}

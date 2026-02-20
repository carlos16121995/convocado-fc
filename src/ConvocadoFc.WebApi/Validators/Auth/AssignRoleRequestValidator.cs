using ConvocadoFc.WebApi.Models.Auth;
using FluentValidation;

namespace ConvocadoFc.WebApi.Validators.Auth;

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

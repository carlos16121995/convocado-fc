using ConvocadoFc.Application.Handlers.Modules.Authentication.Models;
using ConvocadoFc.WebApi.Modules.Authentication.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Modules.Authentication.Validators;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(request => request.CurrentPassword)
            .NotEmpty();

        RuleFor(request => request.NewPassword)
            .NotEmpty()
            .Matches(AuthConstants.PasswordRegex)
            .WithMessage("A senha não atende aos requisitos de complexidade.");
    }
}

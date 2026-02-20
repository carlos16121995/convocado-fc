using ConvocadoFc.Application.Handlers.Modules.Authentication.Models;
using ConvocadoFc.WebApi.Modules.Authentication.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Modules.Authentication.Validators;

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(request => request.UserId)
            .NotEmpty();

        RuleFor(request => request.Token)
            .NotEmpty();

        RuleFor(request => request.NewPassword)
            .NotEmpty()
            .Matches(AuthConstants.PasswordRegex)
            .WithMessage("A senha não atende aos requisitos de complexidade.");
    }
}

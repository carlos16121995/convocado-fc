using ConvocadoFc.Application.Abstractions.Authentication;
using ConvocadoFc.WebApi.Models.Auth;
using FluentValidation;

namespace ConvocadoFc.WebApi.Validators.Auth;

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

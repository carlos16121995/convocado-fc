using ConvocadoFc.Application.Abstractions.Authentication;
using ConvocadoFc.WebApi.Models.Auth;
using FluentValidation;

namespace ConvocadoFc.WebApi.Validators.Auth;

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

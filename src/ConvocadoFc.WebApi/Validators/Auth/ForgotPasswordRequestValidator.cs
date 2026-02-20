using ConvocadoFc.WebApi.Models.Auth;
using FluentValidation;

namespace ConvocadoFc.WebApi.Validators.Auth;

public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress();
    }
}

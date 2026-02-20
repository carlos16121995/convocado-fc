using ConvocadoFc.WebApi.Modules.Authentication.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Modules.Authentication.Validators;

public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress();
    }
}

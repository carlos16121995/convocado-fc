using ConvocadoFc.WebApi.Modules.Authentication.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Modules.Authentication.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(request => request.Password)
            .NotEmpty();
    }
}

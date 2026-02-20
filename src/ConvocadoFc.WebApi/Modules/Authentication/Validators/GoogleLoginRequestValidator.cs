using ConvocadoFc.WebApi.Modules.Authentication.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Modules.Authentication.Validators;

public sealed class GoogleLoginRequestValidator : AbstractValidator<GoogleLoginRequest>
{
    public GoogleLoginRequestValidator()
    {
        RuleFor(request => request.IdToken)
            .NotEmpty();

        When(request => request.Phone is not null, () =>
        {
            RuleFor(request => request.Phone)
                .NotEmpty()
                .MaximumLength(20);
        });
    }
}

using ConvocadoFc.WebApi.Models.Auth;
using FluentValidation;

namespace ConvocadoFc.WebApi.Validators.Auth;

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

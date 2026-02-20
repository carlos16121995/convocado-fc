using ConvocadoFc.Application.Handlers.Modules.Authentication.Models;
using ConvocadoFc.WebApi.Modules.Users.Models;
using FluentValidation;

namespace ConvocadoFc.WebApi.Modules.Users.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(request => request.Phone)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(request => request.Password)
            .NotEmpty()
            .Matches(AuthConstants.PasswordRegex)
            .WithMessage("A senha não atende aos requisitos de complexidade.");

        RuleFor(request => request.Address)
            .MaximumLength(300);

        RuleFor(request => request.ProfilePhotoUrl)
            .MaximumLength(500);
    }
}

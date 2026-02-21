using System.Text.RegularExpressions;

using ConvocadoFc.Application.Handlers.Modules.Authentication.Models;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;

using Microsoft.AspNetCore.Identity;

namespace ConvocadoFc.Infrastructure.Modules.Authentication;

public sealed class RegexPasswordValidator : IPasswordValidator<ApplicationUser>
{
    private static readonly Regex PasswordRegex = new(AuthConstants.PasswordRegex, RegexOptions.Compiled);

    public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || !PasswordRegex.IsMatch(password))
        {
            return Task.FromResult(IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordRegex",
                Description = "A senha não atende aos requisitos de complexidade."
            }));
        }

        return Task.FromResult(IdentityResult.Success);
    }
}

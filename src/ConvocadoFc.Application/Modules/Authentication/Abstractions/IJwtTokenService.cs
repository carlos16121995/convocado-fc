using ConvocadoFc.Domain.Modules.Users.Identity;

using System.Collections.Generic;

namespace ConvocadoFc.Application.Modules.Authentication.Abstractions;

public interface IJwtTokenService
{
    string CreateToken(ApplicationUser user, IEnumerable<string> roles);
}

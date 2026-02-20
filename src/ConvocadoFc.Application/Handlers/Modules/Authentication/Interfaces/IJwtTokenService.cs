using System.Collections.Generic;

using ConvocadoFc.Domain.Models.Modules.Users.Identity;

namespace ConvocadoFc.Application.Handlers.Modules.Authentication.Interfaces;

public interface IJwtTokenService
{
    string CreateToken(ApplicationUser user, IEnumerable<string> roles);
}

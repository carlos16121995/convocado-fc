using ConvocadoFc.Domain.Identity;

using System.Collections.Generic;

namespace ConvocadoFc.Application.Abstractions.Authentication;

public interface IJwtTokenService
{
    string CreateToken(ApplicationUser user, IEnumerable<string> roles);
}

using System.Security.Claims;

namespace ConvocadoFc.WebApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool TryGetUserId(this ClaimsPrincipal user, out Guid userId)
    {
        userId = Guid.Empty;
        var rawId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return rawId is not null && Guid.TryParse(rawId, out userId);
    }
}

using Microsoft.AspNetCore.Http;

namespace ConvocadoFc.Infrastructure.Modules.Authentication;

public sealed class AuthCookieOptions
{
    public string Path { get; init; } = "/";
    public string? Domain { get; init; }
    public bool Secure { get; init; } = true;
    public SameSiteMode SameSite { get; init; } = SameSiteMode.Strict;
}

namespace ConvocadoFc.Infrastructure.Modules.Authentication;

public sealed class RefreshTokenOptions
{
    public string CookieName { get; init; } = "refresh_token";
    public int ExpirationDays { get; init; } = 30;
    public string KeyPrefix { get; init; } = "auth:refresh:";
}

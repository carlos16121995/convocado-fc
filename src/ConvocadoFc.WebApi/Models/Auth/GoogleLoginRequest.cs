namespace ConvocadoFc.WebApi.Models.Auth;

public sealed record GoogleLoginRequest(string IdToken, string? Phone);

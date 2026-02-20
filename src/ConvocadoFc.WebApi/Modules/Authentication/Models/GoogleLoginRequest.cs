namespace ConvocadoFc.WebApi.Modules.Authentication.Models;

public sealed record GoogleLoginRequest(string IdToken, string? Phone);

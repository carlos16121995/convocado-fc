namespace ConvocadoFc.WebApi.Models.Auth;

/// <summary>
/// Solicitação de autenticação via Google (legado).
/// </summary>
/// <param name="IdToken">Token de identidade do Google.</param>
/// <param name="Phone">Telefone para completar cadastro quando necessário.</param>
public sealed record GoogleLoginRequest(string IdToken, string? Phone);

namespace ConvocadoFc.WebApi.Models.Auth;

/// <summary>
/// Solicitação de autenticação com e-mail e senha (legado).
/// </summary>
/// <param name="Email">E-mail do usuário.</param>
/// <param name="Password">Senha do usuário.</param>
public sealed record LoginRequest(string Email, string Password);

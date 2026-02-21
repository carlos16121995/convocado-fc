namespace ConvocadoFc.WebApi.Modules.Authentication.Models;

/// <summary>
/// Solicitação de autenticação com e-mail e senha.
/// </summary>
/// <param name="Email">E-mail do usuário.</param>
/// <param name="Password">Senha do usuário.</param>
public sealed record LoginRequest(string Email, string Password);

namespace ConvocadoFc.WebApi.Modules.Authentication.Models;

/// <summary>
/// Solicitação de recuperação de senha.
/// </summary>
/// <param name="Email">E-mail do usuário para envio das instruções.</param>
public sealed record ForgotPasswordRequest(string Email);

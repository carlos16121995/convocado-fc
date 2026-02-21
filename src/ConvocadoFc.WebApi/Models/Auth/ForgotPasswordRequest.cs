namespace ConvocadoFc.WebApi.Models.Auth;

/// <summary>
/// Solicitação de recuperação de senha (legado).
/// </summary>
/// <param name="Email">E-mail do usuário para envio das instruções.</param>
public sealed record ForgotPasswordRequest(string Email);

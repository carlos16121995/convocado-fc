namespace ConvocadoFc.WebApi.Modules.Authentication.Models;

/// <summary>
/// Solicitação para alteração de senha.
/// </summary>
/// <param name="CurrentPassword">Senha atual do usuário.</param>
/// <param name="NewPassword">Nova senha do usuário.</param>
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

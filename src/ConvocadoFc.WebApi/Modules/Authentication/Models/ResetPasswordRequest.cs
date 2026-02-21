namespace ConvocadoFc.WebApi.Modules.Authentication.Models;

/// <summary>
/// Solicitação para redefinir a senha.
/// </summary>
/// <param name="UserId">Identificador do usuário.</param>
/// <param name="NewPassword">Nova senha do usuário.</param>
public sealed record ResetPasswordRequest(Guid UserId, string NewPassword);

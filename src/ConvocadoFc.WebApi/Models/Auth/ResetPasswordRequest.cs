using System;

namespace ConvocadoFc.WebApi.Models.Auth;

/// <summary>
/// Solicitação para redefinir a senha (legado).
/// </summary>
/// <param name="UserId">Identificador do usuário.</param>
/// <param name="NewPassword">Nova senha do usuário.</param>
public sealed record ResetPasswordRequest(Guid UserId, string NewPassword);

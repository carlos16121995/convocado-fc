using System;

namespace ConvocadoFc.WebApi.Models.Auth;

/// <summary>
/// Solicitação para remover uma role do usuário (legado).
/// </summary>
/// <param name="Role">Nome da role a ser removida.</param>
public sealed record RemoveRoleRequest(string Role);

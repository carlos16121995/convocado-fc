using System;

namespace ConvocadoFc.WebApi.Models.Auth;

/// <summary>
/// Solicitação para atribuir uma role ao usuário (legado).
/// </summary>
/// <param name="Role">Nome da role a ser atribuída.</param>
public sealed record AssignRoleRequest(string Role);

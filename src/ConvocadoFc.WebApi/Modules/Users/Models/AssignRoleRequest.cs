namespace ConvocadoFc.WebApi.Modules.Users.Models;

/// <summary>
/// Solicitação para atribuir uma role ao usuário.
/// </summary>
/// <param name="Role">Nome da role a ser atribuída.</param>
public sealed record AssignRoleRequest(string Role);

namespace ConvocadoFc.WebApi.Modules.Users.Models;

/// <summary>
/// Solicitação para remover uma role do usuário.
/// </summary>
/// <param name="Role">Nome da role a ser removida.</param>
public sealed record RemoveRoleRequest(string Role);

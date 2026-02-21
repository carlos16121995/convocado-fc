namespace ConvocadoFc.WebApi.Modules.Teams.Models;

/// <summary>
/// Solicitação para atribuir moderador ao time.
/// </summary>
/// <param name="UserId">Identificador do usuário a ser promovido.</param>
public sealed record AssignModeratorRequest(
    Guid UserId
);

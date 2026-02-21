namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record RemovePlayerCommand(
    Guid TeamId,
    Guid UserId,
    Guid RemovedByUserId,
    bool IsSystemAdmin
);

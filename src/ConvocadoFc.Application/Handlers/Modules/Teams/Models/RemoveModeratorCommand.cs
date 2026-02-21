namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record RemoveModeratorCommand(
    Guid TeamId,
    Guid UserId,
    Guid RemovedByUserId,
    bool IsSystemAdmin
);

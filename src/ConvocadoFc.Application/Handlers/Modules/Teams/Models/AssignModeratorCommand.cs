namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record AssignModeratorCommand(
    Guid TeamId,
    Guid UserId,
    Guid AssignedByUserId,
    bool IsSystemAdmin
);

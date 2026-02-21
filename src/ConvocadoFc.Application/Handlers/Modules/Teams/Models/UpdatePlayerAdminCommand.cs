namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record UpdatePlayerAdminCommand(
    Guid TeamId,
    Guid UserId,
    Guid UpdatedByUserId,
    bool? IsFeeExempt,
    bool IsSystemAdmin
);

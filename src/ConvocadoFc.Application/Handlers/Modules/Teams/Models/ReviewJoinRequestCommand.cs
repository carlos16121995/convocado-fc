namespace ConvocadoFc.Application.Handlers.Modules.Teams.Models;

public sealed record ReviewJoinRequestCommand(
    Guid RequestId,
    Guid ReviewedByUserId,
    bool Approve,
    bool IsSystemAdmin
);

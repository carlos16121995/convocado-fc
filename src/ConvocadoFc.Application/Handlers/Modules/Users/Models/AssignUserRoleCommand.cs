namespace ConvocadoFc.Application.Handlers.Modules.Users.Models;

public sealed record AssignUserRoleCommand(
    Guid UserId,
    string Role,
    bool IsMaster,
    bool IsAdmin
);

namespace ConvocadoFc.Application.Handlers.Modules.Users.Models;

public sealed record RemoveUserRoleCommand(
    Guid UserId,
    string Role,
    bool IsMaster,
    bool IsAdmin
);

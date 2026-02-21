namespace ConvocadoFc.Application.Handlers.Modules.Authentication.Models;

public sealed record AuthUserDto(
    Guid UserId,
    string Email,
    string FullName,
    bool EmailConfirmed,
    IReadOnlyCollection<string> Roles
);

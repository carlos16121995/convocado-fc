namespace ConvocadoFc.Application.Handlers.Modules.Users.Models;

public sealed record RegisterUserCommand(
    string Name,
    string Email,
    string Phone,
    string Password,
    string? Address,
    string? ProfilePhotoUrl
);

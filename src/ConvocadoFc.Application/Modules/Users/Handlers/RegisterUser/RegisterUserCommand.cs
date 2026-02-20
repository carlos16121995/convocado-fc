namespace ConvocadoFc.Application.Modules.Users.Handlers.RegisterUser;

public sealed record RegisterUserCommand(
    string Name,
    string Email,
    string Phone,
    string Password,
    string? Address,
    string? ProfilePhotoUrl
);

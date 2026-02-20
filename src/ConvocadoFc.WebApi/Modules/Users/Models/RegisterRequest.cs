namespace ConvocadoFc.WebApi.Modules.Users.Models;

public sealed record RegisterRequest(
    string Name,
    string Email,
    string Phone,
    string Password,
    string? Address,
    string? ProfilePhotoUrl
);

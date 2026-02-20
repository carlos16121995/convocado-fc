namespace ConvocadoFc.WebApi.Models.Auth;

public sealed record RegisterRequest(
    string Name,
    string Email,
    string Phone,
    string Password,
    string? Address,
    string? ProfilePhotoUrl
);

namespace ConvocadoFc.Application.Handlers.Modules.Authentication.Models;

public sealed record GoogleLoginCommand(
    string Email,
    string? FullName,
    string? Phone,
    bool EmailVerified
);

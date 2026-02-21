namespace ConvocadoFc.Application.Handlers.Modules.Authentication.Models;

public sealed record ConfirmEmailCommand(
    Guid UserId,
    string Token
);

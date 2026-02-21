namespace ConvocadoFc.Application.Handlers.Modules.Authentication.Models;

public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
);

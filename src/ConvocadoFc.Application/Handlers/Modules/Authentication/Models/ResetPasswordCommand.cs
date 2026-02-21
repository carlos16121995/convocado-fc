namespace ConvocadoFc.Application.Handlers.Modules.Authentication.Models;

public sealed record ResetPasswordCommand(
    Guid UserId,
    string Token,
    string NewPassword
);

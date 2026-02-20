using System;

namespace ConvocadoFc.WebApi.Modules.Authentication.Models;

public sealed record ResetPasswordRequest(Guid UserId, string Token, string NewPassword);

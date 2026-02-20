using System;

namespace ConvocadoFc.WebApi.Models.Auth;

public sealed record ResetPasswordRequest(Guid UserId, string Token, string NewPassword);

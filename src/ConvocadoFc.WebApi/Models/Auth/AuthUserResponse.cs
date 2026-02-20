using System;
using System.Collections.Generic;

namespace ConvocadoFc.WebApi.Models.Auth;

public sealed record AuthUserResponse(
    Guid UserId,
    string Email,
    string Name,
    bool EmailConfirmed,
    IReadOnlyCollection<string> Roles
);

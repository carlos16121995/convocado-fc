using System;

namespace ConvocadoFc.Application.Handlers.Modules.Authentication.Models;

public sealed record RefreshTokenDescriptor(
    Guid TokenId,
    Guid UserId,
    string TokenHash,
    string SecurityStamp,
    DateTimeOffset ExpiresAt
);

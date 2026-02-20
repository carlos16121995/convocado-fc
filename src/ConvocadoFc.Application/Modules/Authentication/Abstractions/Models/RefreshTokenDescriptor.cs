using System;

namespace ConvocadoFc.Application.Modules.Authentication.Abstractions.Models;

public sealed record RefreshTokenDescriptor(
    Guid TokenId,
    Guid UserId,
    string TokenHash,
    string SecurityStamp,
    DateTimeOffset ExpiresAt
);

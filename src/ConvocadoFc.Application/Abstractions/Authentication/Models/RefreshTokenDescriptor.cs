using System;

namespace ConvocadoFc.Application.Abstractions.Authentication.Models;

public sealed record RefreshTokenDescriptor(
    Guid TokenId,
    Guid UserId,
    string TokenHash,
    string SecurityStamp,
    DateTimeOffset ExpiresAt
);

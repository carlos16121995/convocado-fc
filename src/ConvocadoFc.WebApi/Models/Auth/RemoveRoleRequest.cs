using System;

namespace ConvocadoFc.WebApi.Models.Auth;

public sealed record RemoveRoleRequest(Guid UserId, string Role);

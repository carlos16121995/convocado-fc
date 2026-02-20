using System;

namespace ConvocadoFc.WebApi.Models.Auth;

public sealed record AssignRoleRequest(Guid UserId, string Role);

using System;

namespace ConvocadoFc.WebApi.Modules.Users.Models;

public sealed record AssignRoleRequest(Guid UserId, string Role);

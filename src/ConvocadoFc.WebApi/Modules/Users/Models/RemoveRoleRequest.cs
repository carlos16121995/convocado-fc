using System;

namespace ConvocadoFc.WebApi.Modules.Users.Models;

public sealed record RemoveRoleRequest(Guid UserId, string Role);

using System.Collections.Generic;

using ConvocadoFc.Domain.Shared;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;

namespace ConvocadoFc.Application.Handlers.Modules.Users.Models;

public sealed record RegisterUserResult(
    RegisterUserStatus Status,
    IReadOnlyCollection<ValidationFailure> Errors,
    ApplicationUser? User,
    IReadOnlyCollection<string> Roles
);

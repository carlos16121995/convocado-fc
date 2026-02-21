using System.Collections.Generic;

using ConvocadoFc.Domain.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;

namespace ConvocadoFc.Application.Modules.Users.Handlers.RegisterUser;

public sealed record RegisterUserResult(
    ERegisterUserStatus Status,
    IReadOnlyCollection<ValidationFailure> Errors,
    ApplicationUser? User,
    IReadOnlyCollection<string> Roles
);

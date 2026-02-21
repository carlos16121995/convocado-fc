using ConvocadoFc.Domain.Shared;

namespace ConvocadoFc.Application.Handlers.Modules.Users.Models;

public sealed record UserRoleOperationResult(
    EUserRoleOperationStatus Status,
    IReadOnlyCollection<ValidationFailure> Errors
)
{
    public static UserRoleOperationResult Success()
        => new(EUserRoleOperationStatus.Success, Array.Empty<ValidationFailure>());

    public static UserRoleOperationResult Failure(EUserRoleOperationStatus status, IReadOnlyCollection<ValidationFailure>? errors = null)
        => new(status, errors ?? Array.Empty<ValidationFailure>());
}

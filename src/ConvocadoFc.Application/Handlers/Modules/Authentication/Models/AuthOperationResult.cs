using ConvocadoFc.Domain.Shared;

namespace ConvocadoFc.Application.Handlers.Modules.Authentication.Models;

public sealed record AuthOperationResult(
    EAuthOperationStatus Status,
    AuthUserDto? User,
    string? AccessToken,
    string? RefreshToken,
    IReadOnlyCollection<ValidationFailure> Errors
)
{
    public static AuthOperationResult Success(AuthUserDto user, string accessToken, string refreshToken)
        => new(EAuthOperationStatus.Success, user, accessToken, refreshToken, Array.Empty<ValidationFailure>());

    public static AuthOperationResult Success()
        => new(EAuthOperationStatus.Success, null, null, null, Array.Empty<ValidationFailure>());

    public static AuthOperationResult Failure(EAuthOperationStatus status, IReadOnlyCollection<ValidationFailure>? errors = null)
        => new(status, null, null, null, errors ?? Array.Empty<ValidationFailure>());
}

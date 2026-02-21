using ConvocadoFc.Application.Handlers.Modules.Authentication.Models;

namespace ConvocadoFc.Application.Handlers.Modules.Authentication.Interfaces;

public interface IAuthHandler
{
    Task<AuthOperationResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken);
    Task<AuthOperationResult> GoogleLoginAsync(GoogleLoginCommand command, CancellationToken cancellationToken);
    Task<AuthOperationResult> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken);
    Task<AuthOperationResult> RevokeRefreshTokenAsync(RevokeRefreshTokenCommand command, CancellationToken cancellationToken);
    Task<AuthOperationResult> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken);
    Task<AuthOperationResult> ForgotPasswordAsync(ForgotPasswordCommand command, CancellationToken cancellationToken);
    Task<AuthOperationResult> ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken);
    Task<AuthOperationResult> ConfirmEmailAsync(ConfirmEmailCommand command, CancellationToken cancellationToken);
}

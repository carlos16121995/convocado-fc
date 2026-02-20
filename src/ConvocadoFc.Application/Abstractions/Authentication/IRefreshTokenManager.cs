using ConvocadoFc.Application.Abstractions.Authentication.Models;
using ConvocadoFc.Domain.Identity;

using System.Threading;
using System.Threading.Tasks;

namespace ConvocadoFc.Application.Abstractions.Authentication;

public interface IRefreshTokenManager
{
    Task<string> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<RefreshTokenDescriptor?> ValidateAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<string?> RotateAsync(string refreshToken, ApplicationUser user, CancellationToken cancellationToken = default);
    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);
}

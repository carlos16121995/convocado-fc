using ConvocadoFc.Application.Modules.Authentication.Abstractions.Models;
using ConvocadoFc.Domain.Modules.Users.Identity;

using System.Threading;
using System.Threading.Tasks;

namespace ConvocadoFc.Application.Modules.Authentication.Abstractions;

public interface IRefreshTokenManager
{
    Task<string> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<RefreshTokenDescriptor?> ValidateAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<string?> RotateAsync(string refreshToken, ApplicationUser user, CancellationToken cancellationToken = default);
    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);
}

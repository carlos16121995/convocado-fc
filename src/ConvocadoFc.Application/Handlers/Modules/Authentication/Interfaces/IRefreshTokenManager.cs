using System.Threading;
using System.Threading.Tasks;

using ConvocadoFc.Application.Handlers.Modules.Authentication.Models;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;

namespace ConvocadoFc.Application.Handlers.Modules.Authentication.Interfaces;

public interface IRefreshTokenManager
{
    Task<string> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<RefreshTokenDescriptor?> ValidateAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<string?> RotateAsync(string refreshToken, ApplicationUser user, CancellationToken cancellationToken = default);
    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);
}

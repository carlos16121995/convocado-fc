using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using ConvocadoFc.Application.Handlers.Modules.Authentication.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Authentication.Models;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ConvocadoFc.Infrastructure.Modules.Authentication;

public sealed class RefreshTokenManager(
    IConnectionMultiplexer redis,
    IOptions<RefreshTokenOptions> options) : IRefreshTokenManager
{
    private readonly IDatabase _db = redis.GetDatabase();
    private readonly RefreshTokenOptions _options = options.Value;

    public async Task<string> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tokenId = Guid.NewGuid();
        var secret = CreateSecret();
        var token = FormatToken(tokenId, secret);

        var descriptor = new RefreshTokenDescriptor(
            tokenId,
            user.Id,
            HashSecret(secret),
            user.SecurityStamp ?? string.Empty,
            DateTimeOffset.UtcNow.AddDays(_options.ExpirationDays));

        await StoreAsync(descriptor, cancellationToken);

        return token;
    }

    public async Task<RefreshTokenDescriptor?> ValidateAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryParse(refreshToken, out var tokenId, out var secret))
        {
            return null;
        }

        var descriptor = await GetAsync(tokenId, cancellationToken);
        if (descriptor is null)
        {
            return null;
        }

        if (descriptor.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            await RemoveAsync(tokenId, cancellationToken);
            return null;
        }

        var incomingHash = HashSecret(secret);
        if (!FixedTimeEquals(incomingHash, descriptor.TokenHash))
        {
            return null;
        }

        return descriptor;
    }

    public async Task<string?> RotateAsync(string refreshToken, ApplicationUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var descriptor = await ValidateAsync(refreshToken, cancellationToken);
        if (descriptor is null)
        {
            return null;
        }

        if (descriptor.UserId != user.Id || descriptor.SecurityStamp != (user.SecurityStamp ?? string.Empty))
        {
            return null;
        }

        await RemoveAsync(descriptor.TokenId, cancellationToken);

        return await CreateAsync(user, cancellationToken);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryParse(refreshToken, out var tokenId, out _))
        {
            return;
        }

        await RemoveAsync(tokenId, cancellationToken);
    }

    private async Task StoreAsync(RefreshTokenDescriptor descriptor, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = BuildKey(descriptor.TokenId);
        var payload = JsonSerializer.Serialize(descriptor);
        var ttl = descriptor.ExpiresAt - DateTimeOffset.UtcNow;
        if (ttl <= TimeSpan.Zero)
        {
            return;
        }

        await _db.StringSetAsync(key, payload, ttl);
    }

    private async Task<RefreshTokenDescriptor?> GetAsync(Guid tokenId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var value = await _db.StringGetAsync(BuildKey(tokenId));
        if (!value.HasValue)
        {
            return null;
        }

        return JsonSerializer.Deserialize<RefreshTokenDescriptor>(value.ToString());
    }

    private async Task RemoveAsync(Guid tokenId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _db.KeyDeleteAsync(BuildKey(tokenId));
    }

    private string BuildKey(Guid tokenId) => $"{_options.KeyPrefix}{tokenId}";

    private static string CreateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Base64UrlEncode(bytes);
    }

    private static string FormatToken(Guid tokenId, string secret) => $"{tokenId}.{secret}";

    private static bool TryParse(string token, out Guid tokenId, out string secret)
    {
        tokenId = Guid.Empty;
        secret = string.Empty;

        var parts = token.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        if (!Guid.TryParse(parts[0], out tokenId))
        {
            return false;
        }

        secret = parts[1];
        return !string.IsNullOrWhiteSpace(secret);
    }

    private static string HashSecret(string secret)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(secret));
        return Convert.ToBase64String(hash);
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private static string Base64UrlEncode(byte[] input)
        => Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

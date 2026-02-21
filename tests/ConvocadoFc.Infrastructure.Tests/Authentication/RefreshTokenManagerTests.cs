using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ConvocadoFc.Application.Handlers.Modules.Authentication.Models;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Infrastructure.Modules.Authentication;
using Microsoft.Extensions.Options;
using Moq;
using StackExchange.Redis;

namespace ConvocadoFc.Infrastructure.Tests.Authentication;

public sealed class RefreshTokenManagerTests
{
    [Fact]
    public async Task CreateAsync_StoresDescriptorAndReturnsToken()
    {
        var storage = new Dictionary<string, string>();
        var db = CreateDatabase(storage);
        var manager = CreateManager(db.Object, new RefreshTokenOptions { ExpirationDays = 7, KeyPrefix = "rt:" });

        var user = new ApplicationUser { Id = Guid.NewGuid(), SecurityStamp = "stamp" };
        var token = await manager.CreateAsync(user, CancellationToken.None);

        Assert.Contains('.', token);
        db.Verify(database => database.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task ValidateAsync_WhenTokenInvalid_ReturnsNull()
    {
        var storage = new Dictionary<string, string>();
        var db = CreateDatabase(storage);
        var manager = CreateManager(db.Object, new RefreshTokenOptions { ExpirationDays = 7, KeyPrefix = "rt:" });

        var result = await manager.ValidateAsync("invalid", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateAsync_WhenExpired_RemovesToken()
    {
        var storage = new Dictionary<string, string>();
        var db = CreateDatabase(storage);
        var manager = CreateManager(db.Object, new RefreshTokenOptions { ExpirationDays = 7, KeyPrefix = "rt:" });

        var tokenId = Guid.NewGuid();
        var descriptor = new RefreshTokenDescriptor(
            tokenId,
            Guid.NewGuid(),
            HashSecret("secret"),
            "stamp",
            DateTimeOffset.UtcNow.AddMinutes(-1));

        storage[$"rt:{tokenId}"] = JsonSerializer.Serialize(descriptor);

        var token = $"{tokenId}.secret";
        var result = await manager.ValidateAsync(token, CancellationToken.None);

        Assert.Null(result);
        Assert.Empty(storage);
    }

    [Fact]
    public async Task ValidateAsync_WhenHashMismatch_ReturnsNull()
    {
        var storage = new Dictionary<string, string>();
        var db = CreateDatabase(storage);
        var manager = CreateManager(db.Object, new RefreshTokenOptions { ExpirationDays = 7, KeyPrefix = "rt:" });

        var tokenId = Guid.NewGuid();
        var descriptor = new RefreshTokenDescriptor(
            tokenId,
            Guid.NewGuid(),
            HashSecret("secret"),
            "stamp",
            DateTimeOffset.UtcNow.AddMinutes(5));

        storage[$"rt:{tokenId}"] = JsonSerializer.Serialize(descriptor);

        var token = $"{tokenId}.different";
        var result = await manager.ValidateAsync(token, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task RotateAsync_WhenDescriptorInvalid_ReturnsNull()
    {
        var storage = new Dictionary<string, string>();
        var db = CreateDatabase(storage);
        var manager = CreateManager(db.Object, new RefreshTokenOptions { ExpirationDays = 7, KeyPrefix = "rt:" });

        var result = await manager.RotateAsync("invalid", new ApplicationUser { Id = Guid.NewGuid() }, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task RotateAsync_WhenValid_ReplacesToken()
    {
        var storage = new Dictionary<string, string>();
        var db = CreateDatabase(storage);
        var manager = CreateManager(db.Object, new RefreshTokenOptions { ExpirationDays = 7, KeyPrefix = "rt:" });

        var user = new ApplicationUser { Id = Guid.NewGuid(), SecurityStamp = "stamp" };
        var tokenId = Guid.NewGuid();
        var secret = "secret";
        var descriptor = new RefreshTokenDescriptor(
            tokenId,
            user.Id,
            HashSecret(secret),
            user.SecurityStamp,
            DateTimeOffset.UtcNow.AddMinutes(5));

        storage[$"rt:{tokenId}"] = JsonSerializer.Serialize(descriptor);

        var rotated = await manager.RotateAsync($"{tokenId}.{secret}", user, CancellationToken.None);

        Assert.NotNull(rotated);
        db.Verify(database => database.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once);
        db.Verify(database => database.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task RevokeAsync_WhenTokenInvalid_DoesNothing()
    {
        var storage = new Dictionary<string, string>();
        var db = CreateDatabase(storage);
        var manager = CreateManager(db.Object, new RefreshTokenOptions { ExpirationDays = 7, KeyPrefix = "rt:" });

        await manager.RevokeAsync("invalid", CancellationToken.None);

        Assert.Empty(storage);
    }

    private static RefreshTokenManager CreateManager(IDatabase db, RefreshTokenOptions options)
    {
        var redis = new Mock<IConnectionMultiplexer>();
        redis.Setup(connection => connection.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(db);

        return new RefreshTokenManager(redis.Object, Options.Create(options));
    }

    private static Mock<IDatabase> CreateDatabase(Dictionary<string, string> storage)
    {
        var db = new Mock<IDatabase>();

        db.Setup(database => database.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .Callback<RedisKey, RedisValue, TimeSpan?, bool, When, CommandFlags>((key, value, _, _, _, _) => storage[key.ToString()] = value!)
            .ReturnsAsync(true);

        db.Setup(database => database.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisKey key, CommandFlags _) => storage.TryGetValue(key.ToString(), out var value) ? (RedisValue)value : RedisValue.Null);

        db.Setup(database => database.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .Callback<RedisKey, CommandFlags>((key, _) => storage.Remove(key.ToString()))
            .ReturnsAsync(true);

        return db;
    }

    private static string HashSecret(string secret)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(secret));
        return Convert.ToBase64String(hash);
    }
}

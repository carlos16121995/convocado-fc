using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Implementations;
using ConvocadoFc.Domain.Models.Modules.Subscriptions;
using ConvocadoFc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConvocadoFc.Application.Tests;

public sealed class SubscriptionAccessServiceTests
{
    [Fact]
    public async Task GetAccessInfoAsync_WhenNoActiveSubscription_ReturnsDenied()
    {
        var ownerUserId = Guid.NewGuid();
        await using var context = CreateContext();

        var service = new SubscriptionAccessService(context);

        var result = await service.GetAccessInfoAsync(ownerUserId, CancellationToken.None);

        Assert.False(result.HasActiveSubscription);
        Assert.Null(result.PlanId);
        Assert.Null(result.PlanCode);
        Assert.Null(result.Capacity);
    }

    [Fact]
    public async Task GetAccessInfoAsync_WhenActiveSubscription_ReturnsPlanInfo()
    {
        var ownerUserId = Guid.NewGuid();
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Pro",
            Code = "PRO",
            Currency = "BRL",
            Price = 99,
            MaxTeams = 5,
            MaxMembersPerTeam = 20,
            IsActive = true
        };

        await using var context = CreateContext();
        context.Plans.Add(plan);
        context.Subscriptions.Add(new Subscription
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            PlanId = plan.Id,
            Status = ESubscriptionStatus.Active,
            StartsAt = DateTimeOffset.UtcNow.AddDays(-1)
        });
        await context.SaveChangesAsync();

        var service = new SubscriptionAccessService(context);

        var result = await service.GetAccessInfoAsync(ownerUserId, CancellationToken.None);

        Assert.True(result.HasActiveSubscription);
        Assert.Equal(plan.Id, result.PlanId);
        Assert.Equal(plan.Code, result.PlanCode);
        Assert.NotNull(result.Capacity);
        Assert.Equal(5, result.Capacity!.MaxTeams);
        Assert.Equal(20, result.Capacity.MaxMembersPerTeam);
    }

    [Fact]
    public async Task CanCreateTeamAsync_WhenActiveSubscription_ReturnsTrue()
    {
        var ownerUserId = Guid.NewGuid();
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Pro",
            Code = "PRO",
            Currency = "BRL",
            Price = 99,
            MaxTeams = 5,
            MaxMembersPerTeam = 20,
            IsActive = true
        };

        await using var context = CreateContext();
        context.Plans.Add(plan);
        context.Subscriptions.Add(new Subscription
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            PlanId = plan.Id,
            Status = ESubscriptionStatus.Active,
            StartsAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new SubscriptionAccessService(context);

        var result = await service.CanCreateTeamAsync(ownerUserId, CancellationToken.None);

        Assert.True(result);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}

using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Implementations;
using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;
using ConvocadoFc.Domain.Models.Modules.Subscriptions;
using ConvocadoFc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConvocadoFc.Application.Tests;

public sealed class PlanManagementHandlerTests
{
    [Fact]
    public async Task CreatePlanAsync_WhenInvalidCapacity_ReturnsInvalidCapacity()
    {
        await using var context = CreateContext();
        var handler = new PlanManagementHandler(context);

        var result = await handler.CreatePlanAsync(new CreatePlanCommand(
            "Plan",
            "PLAN",
            10,
            "BRL",
            null,
            null,
            false,
            true),
            CancellationToken.None);

        Assert.Equal(EPlanOperationStatus.InvalidCapacity, result.Status);
        Assert.Null(result.Plan);
    }

    [Fact]
    public async Task CreatePlanAsync_WhenCodeExists_ReturnsConflict()
    {
        await using var context = CreateContext();
        context.Plans.Add(new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Plan",
            Code = "PLAN",
            Currency = "BRL",
            MaxTeams = 1,
            MaxMembersPerTeam = 1,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var handler = new PlanManagementHandler(context);

        var result = await handler.CreatePlanAsync(new CreatePlanCommand(
            "Plan2",
            "PLAN",
            10,
            "BRL",
            1,
            1,
            false,
            true),
            CancellationToken.None);

        Assert.Equal(EPlanOperationStatus.CodeAlreadyExists, result.Status);
    }

    [Fact]
    public async Task CreatePlanAsync_WhenNameExists_ReturnsConflict()
    {
        await using var context = CreateContext();
        context.Plans.Add(new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Plan",
            Code = "PLAN",
            Currency = "BRL",
            MaxTeams = 1,
            MaxMembersPerTeam = 1,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var handler = new PlanManagementHandler(context);

        var result = await handler.CreatePlanAsync(new CreatePlanCommand(
            "Plan",
            "PLAN2",
            10,
            "BRL",
            1,
            1,
            false,
            true),
            CancellationToken.None);

        Assert.Equal(EPlanOperationStatus.NameAlreadyExists, result.Status);
    }

    [Fact]
    public async Task CreatePlanAsync_WhenSuccessful_ReturnsPlan()
    {
        await using var context = CreateContext();
        var handler = new PlanManagementHandler(context);

        var result = await handler.CreatePlanAsync(new CreatePlanCommand(
            " Plan ",
            " plan ",
            10,
            string.Empty,
            1,
            1,
            false,
            true),
            CancellationToken.None);

        Assert.Equal(EPlanOperationStatus.Success, result.Status);
        Assert.NotNull(result.Plan);
        Assert.Equal("PLAN", result.Plan!.Code);
        Assert.Equal("Plan", result.Plan.Name);
        Assert.Equal("BRL", result.Plan.Currency);
    }

    [Fact]
    public async Task UpdatePlanAsync_WhenInvalidCapacity_ReturnsInvalidCapacity()
    {
        await using var context = CreateContext();
        var handler = new PlanManagementHandler(context);

        var result = await handler.UpdatePlanAsync(new UpdatePlanCommand(
            Guid.NewGuid(),
            "Plan",
            "PLAN",
            10,
            "BRL",
            null,
            null,
            false,
            true),
            CancellationToken.None);

        Assert.Equal(EPlanOperationStatus.InvalidCapacity, result.Status);
    }

    [Fact]
    public async Task UpdatePlanAsync_WhenNotFound_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var handler = new PlanManagementHandler(context);

        var result = await handler.UpdatePlanAsync(new UpdatePlanCommand(
            Guid.NewGuid(),
            "Plan",
            "PLAN",
            10,
            "BRL",
            1,
            1,
            false,
            true),
            CancellationToken.None);

        Assert.Equal(EPlanOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task UpdatePlanAsync_WhenCodeExists_ReturnsConflict()
    {
        var planId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Plans.Add(new Plan { Id = planId, Name = "Plan", Code = "PLAN", Currency = "BRL", MaxTeams = 1, MaxMembersPerTeam = 1, IsActive = true });
        context.Plans.Add(new Plan { Id = Guid.NewGuid(), Name = "Plan2", Code = "CODE", Currency = "BRL", MaxTeams = 1, MaxMembersPerTeam = 1, IsActive = true });
        await context.SaveChangesAsync();

        var handler = new PlanManagementHandler(context);

        var result = await handler.UpdatePlanAsync(new UpdatePlanCommand(
            planId,
            "Plan",
            "CODE",
            10,
            "BRL",
            1,
            1,
            false,
            true),
            CancellationToken.None);

        Assert.Equal(EPlanOperationStatus.CodeAlreadyExists, result.Status);
    }

    [Fact]
    public async Task UpdatePlanAsync_WhenNameExists_ReturnsConflict()
    {
        var planId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Plans.Add(new Plan { Id = planId, Name = "Plan", Code = "PLAN", Currency = "BRL", MaxTeams = 1, MaxMembersPerTeam = 1, IsActive = true });
        context.Plans.Add(new Plan { Id = Guid.NewGuid(), Name = "Plan2", Code = "CODE", Currency = "BRL", MaxTeams = 1, MaxMembersPerTeam = 1, IsActive = true });
        await context.SaveChangesAsync();

        var handler = new PlanManagementHandler(context);

        var result = await handler.UpdatePlanAsync(new UpdatePlanCommand(
            planId,
            "Plan2",
            "PLAN",
            10,
            "BRL",
            1,
            1,
            false,
            true),
            CancellationToken.None);

        Assert.Equal(EPlanOperationStatus.NameAlreadyExists, result.Status);
    }

    [Fact]
    public async Task UpdatePlanAsync_WhenSuccessful_UpdatesPlan()
    {
        var planId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Plans.Add(new Plan { Id = planId, Name = "Plan", Code = "PLAN", Currency = "BRL", MaxTeams = 1, MaxMembersPerTeam = 1, IsActive = true });
        await context.SaveChangesAsync();

        var handler = new PlanManagementHandler(context);

        var result = await handler.UpdatePlanAsync(new UpdatePlanCommand(
            planId,
            " Plan2 ",
            " code ",
            20,
            "usd",
            2,
            3,
            false,
            false),
            CancellationToken.None);

        Assert.Equal(EPlanOperationStatus.Success, result.Status);
        Assert.NotNull(result.Plan);
        Assert.Equal("Plan2", result.Plan!.Name);
        Assert.Equal("CODE", result.Plan.Code);
        Assert.Equal("USD", result.Plan.Currency);
        Assert.False(result.Plan.IsActive);
    }

    [Fact]
    public async Task DeactivatePlanAsync_WhenNotFound_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var handler = new PlanManagementHandler(context);

        var result = await handler.DeactivatePlanAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(EPlanOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task DeactivatePlanAsync_WhenSuccessful_DisablesPlan()
    {
        var planId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Plans.Add(new Plan { Id = planId, Name = "Plan", Code = "PLAN", Currency = "BRL", MaxTeams = 1, MaxMembersPerTeam = 1, IsActive = true });
        await context.SaveChangesAsync();

        var handler = new PlanManagementHandler(context);

        var result = await handler.DeactivatePlanAsync(planId, CancellationToken.None);

        Assert.Equal(EPlanOperationStatus.Success, result.Status);
        Assert.NotNull(result.Plan);
        Assert.False(result.Plan!.IsActive);
    }

    [Fact]
    public async Task ListPlansAsync_ReturnsOrderedPlans()
    {
        await using var context = CreateContext();
        context.Plans.Add(new Plan { Id = Guid.NewGuid(), Name = "B", Code = "B", Currency = "BRL", MaxTeams = 1, MaxMembersPerTeam = 1, IsActive = true });
        context.Plans.Add(new Plan { Id = Guid.NewGuid(), Name = "A", Code = "A", Currency = "BRL", MaxTeams = 1, MaxMembersPerTeam = 1, IsActive = true });
        await context.SaveChangesAsync();

        var handler = new PlanManagementHandler(context);
        var result = await handler.ListPlansAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("A", result.First().Name);
    }

    [Fact]
    public async Task GetPlanAsync_WhenExists_ReturnsPlan()
    {
        var planId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Plans.Add(new Plan { Id = planId, Name = "Plan", Code = "PLAN", Currency = "BRL", MaxTeams = 1, MaxMembersPerTeam = 1, IsActive = true });
        await context.SaveChangesAsync();

        var handler = new PlanManagementHandler(context);
        var result = await handler.GetPlanAsync(planId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Plan", result!.Name);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}

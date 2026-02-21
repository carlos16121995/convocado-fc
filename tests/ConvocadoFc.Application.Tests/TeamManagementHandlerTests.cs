using ConvocadoFc.Application.Handlers.Modules.Teams.Implementations;
using ConvocadoFc.Application.Handlers.Modules.Teams.Models;
using ConvocadoFc.Domain.Models.Modules.Teams;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;
using ConvocadoFc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ConvocadoFc.Application.Tests;

public sealed class TeamManagementHandlerTests
{
    [Fact]
    public async Task CreateTeamAsync_WhenNotSystemAdmin_ReturnsForbidden()
    {
        await using var context = CreateContext();
        var handler = new TeamManagementHandler(context);

        var result = await handler.CreateTeamAsync(new CreateTeamCommand(
            Guid.NewGuid(),
            "Time",
            "Campo",
            null,
            null,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamOperationStatus.Forbidden, result.Status);
        Assert.Null(result.Team);
    }

    [Fact]
    public async Task CreateTeamAsync_WhenInvalidData_ReturnsInvalidData()
    {
        await using var context = CreateContext();
        var handler = new TeamManagementHandler(context);

        var result = await handler.CreateTeamAsync(new CreateTeamCommand(
            Guid.NewGuid(),
            "",
            "",
            null,
            null,
            null,
            null,
            true),
            CancellationToken.None);

        Assert.Equal(ETeamOperationStatus.InvalidData, result.Status);
        Assert.Null(result.Team);
    }

    [Fact]
    public async Task CreateTeamAsync_WhenOwnerMissing_ReturnsUserNotFound()
    {
        await using var context = CreateContext();
        var handler = new TeamManagementHandler(context);

        var result = await handler.CreateTeamAsync(new CreateTeamCommand(
            Guid.NewGuid(),
            "Time",
            "Campo",
            null,
            null,
            null,
            null,
            true),
            CancellationToken.None);

        Assert.Equal(ETeamOperationStatus.UserNotFound, result.Status);
        Assert.Null(result.Team);
    }

    [Fact]
    public async Task CreateTeamAsync_WhenNameAlreadyExists_ReturnsConflict()
    {
        var ownerId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Users.Add(CreateUser(ownerId));
        context.Teams.Add(new Team
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerId,
            Name = "Time",
            HomeFieldName = "Campo"
        });
        await context.SaveChangesAsync();

        var handler = new TeamManagementHandler(context);

        var result = await handler.CreateTeamAsync(new CreateTeamCommand(
            ownerId,
            "Time",
            "Campo",
            null,
            null,
            null,
            null,
            true),
            CancellationToken.None);

        Assert.Equal(ETeamOperationStatus.NameAlreadyExists, result.Status);
        Assert.Null(result.Team);
    }

    [Fact]
    public async Task CreateTeamAsync_WhenSuccessful_CreatesTeamSettingsAndOwnerMember()
    {
        var ownerId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Users.Add(CreateUser(ownerId));
        await context.SaveChangesAsync();

        var handler = new TeamManagementHandler(context);

        var result = await handler.CreateTeamAsync(new CreateTeamCommand(
            ownerId,
            " Time ",
            " Campo ",
            " Rua ",
            1.2m,
            3.4m,
            " crest ",
            true),
            CancellationToken.None);

        Assert.Equal(ETeamOperationStatus.Success, result.Status);
        Assert.NotNull(result.Team);
        Assert.Equal("Time", result.Team!.Name);
        Assert.Equal("Campo", result.Team.HomeFieldName);
        Assert.Equal("Rua", result.Team.HomeFieldAddress);
        Assert.Equal("crest", result.Team.CrestUrl);

        var teamId = result.Team.Id;
        var settings = await context.TeamSettings.FirstOrDefaultAsync(item => item.TeamId == teamId);
        var member = await context.TeamMembers.FirstOrDefaultAsync(item => item.TeamId == teamId && item.UserId == ownerId);

        Assert.NotNull(settings);
        Assert.NotNull(member);
        Assert.Equal(ETeamMemberRole.Admin, member!.Role);
    }

    [Fact]
    public async Task UpdateTeamAsync_WhenInvalidData_ReturnsInvalidData()
    {
        await using var context = CreateContext();
        var handler = new TeamManagementHandler(context);

        var result = await handler.UpdateTeamAsync(new UpdateTeamCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "",
            "",
            null,
            null,
            null,
            null,
            true,
            true),
            CancellationToken.None);

        Assert.Equal(ETeamOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task UpdateTeamAsync_WhenNotFound_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var handler = new TeamManagementHandler(context);

        var result = await handler.UpdateTeamAsync(new UpdateTeamCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Time",
            "Campo",
            null,
            null,
            null,
            null,
            true,
            true),
            CancellationToken.None);

        Assert.Equal(ETeamOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task UpdateTeamAsync_WhenNotOwnerOrAdmin_ReturnsForbidden()
    {
        var ownerId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Users.Add(CreateUser(ownerId));
        context.Teams.Add(new Team
        {
            Id = teamId,
            OwnerUserId = ownerId,
            Name = "Time",
            HomeFieldName = "Campo"
        });
        await context.SaveChangesAsync();

        var handler = new TeamManagementHandler(context);

        var result = await handler.UpdateTeamAsync(new UpdateTeamCommand(
            teamId,
            Guid.NewGuid(),
            "Time",
            "Campo",
            null,
            null,
            null,
            null,
            true,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamOperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task UpdateTeamAsync_WhenNameAlreadyExists_ReturnsConflict()
    {
        var ownerId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Users.Add(CreateUser(ownerId));
        context.Teams.Add(new Team
        {
            Id = teamId,
            OwnerUserId = ownerId,
            Name = "Time",
            HomeFieldName = "Campo"
        });
        context.Teams.Add(new Team
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerId,
            Name = "Outro",
            HomeFieldName = "Campo"
        });
        await context.SaveChangesAsync();

        var handler = new TeamManagementHandler(context);

        var result = await handler.UpdateTeamAsync(new UpdateTeamCommand(
            teamId,
            ownerId,
            "Outro",
            "Campo",
            null,
            null,
            null,
            null,
            true,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamOperationStatus.NameAlreadyExists, result.Status);
    }

    [Fact]
    public async Task UpdateTeamAsync_WhenSuccessful_UpdatesTeam()
    {
        var ownerId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Users.Add(CreateUser(ownerId));
        context.Teams.Add(new Team
        {
            Id = teamId,
            OwnerUserId = ownerId,
            Name = "Time",
            HomeFieldName = "Campo"
        });
        await context.SaveChangesAsync();

        var handler = new TeamManagementHandler(context);

        var result = await handler.UpdateTeamAsync(new UpdateTeamCommand(
            teamId,
            ownerId,
            " Novo ",
            " Campo ",
            " Rua ",
            1.2m,
            3.4m,
            " crest ",
            false,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamOperationStatus.Success, result.Status);
        Assert.NotNull(result.Team);
        Assert.Equal("Novo", result.Team!.Name);
        Assert.Equal("Campo", result.Team.HomeFieldName);
        Assert.Equal("Rua", result.Team.HomeFieldAddress);
        Assert.Equal("crest", result.Team.CrestUrl);
        Assert.False(result.Team.IsActive);
    }

    [Fact]
    public async Task RemoveTeamAsync_WhenNotFound_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var handler = new TeamManagementHandler(context);

        var result = await handler.RemoveTeamAsync(Guid.NewGuid(), Guid.NewGuid(), true, CancellationToken.None);

        Assert.Equal(ETeamOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task RemoveTeamAsync_WhenNotOwnerOrAdmin_ReturnsForbidden()
    {
        var ownerId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Users.Add(CreateUser(ownerId));
        context.Teams.Add(new Team
        {
            Id = teamId,
            OwnerUserId = ownerId,
            Name = "Time",
            HomeFieldName = "Campo"
        });
        await context.SaveChangesAsync();

        var handler = new TeamManagementHandler(context);

        var result = await handler.RemoveTeamAsync(teamId, Guid.NewGuid(), false, CancellationToken.None);

        Assert.Equal(ETeamOperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task RemoveTeamAsync_WhenSuccessful_DisablesTeam()
    {
        var ownerId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Users.Add(CreateUser(ownerId));
        context.Teams.Add(new Team
        {
            Id = teamId,
            OwnerUserId = ownerId,
            Name = "Time",
            HomeFieldName = "Campo",
            IsActive = true
        });
        await context.SaveChangesAsync();

        var handler = new TeamManagementHandler(context);

        var result = await handler.RemoveTeamAsync(teamId, ownerId, false, CancellationToken.None);

        Assert.Equal(ETeamOperationStatus.Success, result.Status);
        Assert.NotNull(result.Team);
        Assert.False(result.Team!.IsActive);
    }

    [Fact]
    public async Task ListTeamsAsync_AppliesPaginationAndOrdering()
    {
        var ownerId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Users.Add(CreateUser(ownerId));
        context.Teams.Add(new Team { Id = Guid.NewGuid(), OwnerUserId = ownerId, Name = "B", HomeFieldName = "Campo", CreatedAt = DateTimeOffset.UtcNow.AddDays(-1) });
        context.Teams.Add(new Team { Id = Guid.NewGuid(), OwnerUserId = ownerId, Name = "A", HomeFieldName = "Campo", CreatedAt = DateTimeOffset.UtcNow });
        await context.SaveChangesAsync();

        var handler = new TeamManagementHandler(context);

        var result = await handler.ListTeamsAsync(new ListTeamsQuery(
            new PaginationQuery { Page = 1, PageSize = 1, OrderBy = "createdAt" },
            ownerId),
            CancellationToken.None);

        Assert.Equal(2, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal("A", result.Items.First().Name);
    }

    [Fact]
    public async Task GetTeamAsync_WhenExists_ReturnsTeam()
    {
        var ownerId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        await using var context = CreateContext();
        context.Users.Add(CreateUser(ownerId));
        context.Teams.Add(new Team
        {
            Id = teamId,
            OwnerUserId = ownerId,
            Name = "Time",
            HomeFieldName = "Campo"
        });
        await context.SaveChangesAsync();

        var handler = new TeamManagementHandler(context);

        var result = await handler.GetTeamAsync(teamId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Time", result!.Name);
    }

    private static ApplicationUser CreateUser(Guid userId)
        => new()
        {
            Id = userId,
            Email = $"{userId}@local",
            FullName = "User",
            PhoneNumber = "11999999999"
        };

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }
}

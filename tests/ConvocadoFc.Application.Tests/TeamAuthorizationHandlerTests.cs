using ConvocadoFc.Application.Handlers.Modules.Teams.Implementations;
using ConvocadoFc.Application.Handlers.Modules.Teams.Models;
using ConvocadoFc.Domain.Models.Modules.Teams;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConvocadoFc.Application.Tests;

public sealed class TeamAuthorizationHandlerTests
{
    [Fact]
    public async Task ListModeratorsAsync_WhenNotAdminAndNotSystemAdmin_ReturnsEmpty()
    {
        var teamId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = currentUserId,
            Role = ETeamMemberRole.User,
            Status = ETeamMemberStatus.Active
        });
        context.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = Guid.NewGuid(),
            Role = ETeamMemberRole.Moderator,
            Status = ETeamMemberStatus.Active
        });
        await context.SaveChangesAsync();

        var handler = new TeamAuthorizationHandler(context);

        var result = await handler.ListModeratorsAsync(teamId, currentUserId, false, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ListModeratorsAsync_WhenSystemAdmin_ReturnsModerators()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Users.Add(new ApplicationUser
        {
            Id = userId,
            Email = "user@local",
            FullName = "User",
            PhoneNumber = "11999999999"
        });
        context.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = userId,
            Role = ETeamMemberRole.Moderator,
            Status = ETeamMemberStatus.Active
        });
        await context.SaveChangesAsync();

        var handler = new TeamAuthorizationHandler(context);

        var result = await handler.ListModeratorsAsync(teamId, Guid.NewGuid(), true, CancellationToken.None);

        var moderator = Assert.Single(result);
        Assert.Equal(teamId, moderator.TeamId);
        Assert.Equal(userId, moderator.UserId);
        Assert.Equal("User", moderator.FullName);
        Assert.Equal(ETeamMemberRole.Moderator, moderator.Role);
    }

    [Fact]
    public async Task AssignModeratorAsync_WhenInvalidData_ReturnsInvalidData()
    {
        await using var context = CreateContext();
        var handler = new TeamAuthorizationHandler(context);

        var result = await handler.AssignModeratorAsync(new AssignModeratorCommand(
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamAuthorizationOperationStatus.InvalidData, result.Status);
        Assert.Null(result.Moderator);
    }

    [Fact]
    public async Task AssignModeratorAsync_WhenNotAdmin_ReturnsForbidden()
    {
        var teamId = Guid.NewGuid();
        var assignedBy = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = assignedBy,
            Role = ETeamMemberRole.User,
            Status = ETeamMemberStatus.Active
        });
        await context.SaveChangesAsync();

        var handler = new TeamAuthorizationHandler(context);

        var result = await handler.AssignModeratorAsync(new AssignModeratorCommand(
            teamId,
            Guid.NewGuid(),
            assignedBy,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamAuthorizationOperationStatus.Forbidden, result.Status);
        Assert.Null(result.Moderator);
    }

    [Fact]
    public async Task AssignModeratorAsync_WhenMemberNotFound_ReturnsNotFound()
    {
        var teamId = Guid.NewGuid();
        var assignedBy = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = assignedBy,
            Role = ETeamMemberRole.Admin,
            Status = ETeamMemberStatus.Active
        });
        await context.SaveChangesAsync();

        var handler = new TeamAuthorizationHandler(context);

        var result = await handler.AssignModeratorAsync(new AssignModeratorCommand(
            teamId,
            Guid.NewGuid(),
            assignedBy,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamAuthorizationOperationStatus.NotFound, result.Status);
        Assert.Null(result.Moderator);
    }

    [Fact]
    public async Task AssignModeratorAsync_WhenSuccessful_UpdatesRoleAndReturnsModerator()
    {
        var teamId = Guid.NewGuid();
        var assignedBy = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Users.Add(new ApplicationUser
        {
            Id = userId,
            Email = "user@local",
            FullName = "User",
            PhoneNumber = "11999999999"
        });
        context.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = assignedBy,
            Role = ETeamMemberRole.Admin,
            Status = ETeamMemberStatus.Active
        });
        var targetMember = new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = userId,
            Role = ETeamMemberRole.User,
            Status = ETeamMemberStatus.Active
        };
        context.TeamMembers.Add(targetMember);
        await context.SaveChangesAsync();

        var handler = new TeamAuthorizationHandler(context);

        var result = await handler.AssignModeratorAsync(new AssignModeratorCommand(
            teamId,
            userId,
            assignedBy,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamAuthorizationOperationStatus.Success, result.Status);
        Assert.NotNull(result.Moderator);
        Assert.Equal(ETeamMemberRole.Moderator, result.Moderator!.Role);

        var updatedMember = await context.TeamMembers.FirstAsync(member => member.UserId == userId);
        Assert.Equal(ETeamMemberRole.Moderator, updatedMember.Role);
    }

    [Fact]
    public async Task RemoveModeratorAsync_WhenInvalidData_ReturnsInvalidData()
    {
        await using var context = CreateContext();
        var handler = new TeamAuthorizationHandler(context);

        var result = await handler.RemoveModeratorAsync(new RemoveModeratorCommand(
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamAuthorizationOperationStatus.InvalidData, result.Status);
        Assert.Null(result.Moderator);
    }

    [Fact]
    public async Task RemoveModeratorAsync_WhenNotAdmin_ReturnsForbidden()
    {
        var teamId = Guid.NewGuid();
        var removedBy = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = removedBy,
            Role = ETeamMemberRole.User,
            Status = ETeamMemberStatus.Active
        });
        await context.SaveChangesAsync();

        var handler = new TeamAuthorizationHandler(context);

        var result = await handler.RemoveModeratorAsync(new RemoveModeratorCommand(
            teamId,
            Guid.NewGuid(),
            removedBy,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamAuthorizationOperationStatus.Forbidden, result.Status);
        Assert.Null(result.Moderator);
    }

    [Fact]
    public async Task RemoveModeratorAsync_WhenMemberNotFound_ReturnsNotFound()
    {
        var teamId = Guid.NewGuid();
        var removedBy = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = removedBy,
            Role = ETeamMemberRole.Admin,
            Status = ETeamMemberStatus.Active
        });
        await context.SaveChangesAsync();

        var handler = new TeamAuthorizationHandler(context);

        var result = await handler.RemoveModeratorAsync(new RemoveModeratorCommand(
            teamId,
            Guid.NewGuid(),
            removedBy,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamAuthorizationOperationStatus.NotFound, result.Status);
        Assert.Null(result.Moderator);
    }

    [Fact]
    public async Task RemoveModeratorAsync_WhenSuccessful_UpdatesRoleAndReturnsModerator()
    {
        var teamId = Guid.NewGuid();
        var removedBy = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Users.Add(new ApplicationUser
        {
            Id = userId,
            Email = "user@local",
            FullName = "User",
            PhoneNumber = "11999999999"
        });
        context.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = removedBy,
            Role = ETeamMemberRole.Admin,
            Status = ETeamMemberStatus.Active
        });
        var targetMember = new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = userId,
            Role = ETeamMemberRole.Moderator,
            Status = ETeamMemberStatus.Active
        };
        context.TeamMembers.Add(targetMember);
        await context.SaveChangesAsync();

        var handler = new TeamAuthorizationHandler(context);

        var result = await handler.RemoveModeratorAsync(new RemoveModeratorCommand(
            teamId,
            userId,
            removedBy,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamAuthorizationOperationStatus.Success, result.Status);
        Assert.NotNull(result.Moderator);
        Assert.Equal(ETeamMemberRole.User, result.Moderator!.Role);

        var updatedMember = await context.TeamMembers.FirstAsync(member => member.UserId == userId);
        Assert.Equal(ETeamMemberRole.User, updatedMember.Role);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}

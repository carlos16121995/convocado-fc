using ConvocadoFc.Application.Handlers.Modules.Teams.Implementations;
using ConvocadoFc.Application.Handlers.Modules.Teams.Models;
using ConvocadoFc.Domain.Models.Modules.Teams;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;
using ConvocadoFc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ConvocadoFc.Application.Tests;

public sealed class TeamPlayerHandlerTests
{
    [Fact]
    public async Task ListPlayersAsync_WhenNotModerator_ReturnsEmpty()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        await context.SaveChangesAsync();

        var handler = new TeamPlayerHandler(context);

        var result = await handler.ListPlayersAsync(new ListTeamPlayersQuery(
            new PaginationQuery(),
            teamId,
            userId,
            false),
            CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
    }

    [Fact]
    public async Task ListPlayersAsync_WhenModerator_ReturnsProfiles()
    {
        var teamId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.Users.AddRange(
            CreateUser(adminId, "Admin"),
            CreateUser(playerId, "Player"));
        context.TeamMembers.AddRange(
            new TeamMember { Id = Guid.NewGuid(), TeamId = teamId, UserId = adminId, Role = ETeamMemberRole.Admin, Status = ETeamMemberStatus.Active },
            new TeamMember { Id = memberId, TeamId = teamId, UserId = playerId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active, JoinedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero) });
        context.TeamMemberProfiles.Add(new TeamMemberProfile
        {
            TeamMemberId = memberId,
            IsFeeExempt = true,
            PrimaryPosition = EPlayerPosition.Goalkeeper
        });
        await context.SaveChangesAsync();

        var handler = new TeamPlayerHandler(context);

        var result = await handler.ListPlayersAsync(new ListTeamPlayersQuery(
            new PaginationQuery { OrderBy = "joinedAt" },
            teamId,
            adminId,
            false),
            CancellationToken.None);

        Assert.Equal(2, result.TotalItems);
        var player = result.Items.First(item => item.UserId == playerId);
        Assert.True(player.IsFeeExempt);
        Assert.Equal(EPlayerPosition.Goalkeeper, player.PrimaryPosition);
        Assert.Equal("Player", player.FullName);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_WhenInvalidData_ReturnsInvalidData()
    {
        await using var context = CreateContext();
        var handler = new TeamPlayerHandler(context);

        var result = await handler.UpdateMyProfileAsync(new UpdateMyProfileCommand(
            Guid.Empty,
            Guid.Empty,
            null,
            null,
            null,
            null,
            null,
            null),
            CancellationToken.None);

        Assert.Equal(ETeamPlayerOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_WhenMemberNotFound_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var handler = new TeamPlayerHandler(context);

        var result = await handler.UpdateMyProfileAsync(new UpdateMyProfileCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            null,
            null),
            CancellationToken.None);

        Assert.Equal(ETeamPlayerOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_WhenSuccess_CreatesProfile()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.Users.Add(CreateUser(userId, "Player"));
        context.TeamMembers.Add(new TeamMember { Id = memberId, TeamId = teamId, UserId = userId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamPlayerHandler(context);

        var result = await handler.UpdateMyProfileAsync(new UpdateMyProfileCommand(
            teamId,
            userId,
            EPlayerPosition.Forward,
            EPlayerPosition.Defender,
            null,
            null,
            null,
            null),
            CancellationToken.None);

        Assert.Equal(ETeamPlayerOperationStatus.Success, result.Status);
        var profile = await context.TeamMemberProfiles.FirstAsync(item => item.TeamMemberId == memberId);
        Assert.Equal(EPlayerPosition.Forward, profile.PrimaryPosition);
        Assert.Equal(EPlayerPosition.Defender, profile.SecondaryPosition);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_WhenCopyFromTeam_UsesSourcePositions()
    {
        var sourceTeamId = Guid.NewGuid();
        var targetTeamId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sourceMemberId = Guid.NewGuid();
        var targetMemberId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.AddRange(CreateTeam(sourceTeamId), CreateTeam(targetTeamId));
        context.Users.Add(CreateUser(userId, "Player"));
        context.TeamMembers.AddRange(
            new TeamMember { Id = sourceMemberId, TeamId = sourceTeamId, UserId = userId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active },
            new TeamMember { Id = targetMemberId, TeamId = targetTeamId, UserId = userId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active });
        context.TeamMemberProfiles.Add(new TeamMemberProfile
        {
            TeamMemberId = sourceMemberId,
            PrimaryPosition = EPlayerPosition.Midfielder,
            SecondaryPosition = EPlayerPosition.Winger
        });
        await context.SaveChangesAsync();

        var handler = new TeamPlayerHandler(context);

        var result = await handler.UpdateMyProfileAsync(new UpdateMyProfileCommand(
            targetTeamId,
            userId,
            EPlayerPosition.Midfielder,
            EPlayerPosition.Winger,
            null,
            sourceTeamId,
            null,
            null),
            CancellationToken.None);

        Assert.Equal(ETeamPlayerOperationStatus.Success, result.Status);
        var profile = await context.TeamMemberProfiles.FirstAsync(item => item.TeamMemberId == targetMemberId);
        Assert.Equal(EPlayerPosition.Midfielder, profile.PrimaryPosition);
        Assert.Equal(EPlayerPosition.Winger, profile.SecondaryPosition);
        Assert.Equal(sourceTeamId, profile.CopiedFromTeamId);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_WhenHiatusPeriodInvalid_ReturnsInvalidData()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.Users.Add(CreateUser(userId, "Player"));
        context.TeamMembers.Add(new TeamMember { Id = Guid.NewGuid(), TeamId = teamId, UserId = userId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active });
        AddSetting(context, teamId, TeamSettingKeys.MinHiatusDays, "10");
        await context.SaveChangesAsync();

        var handler = new TeamPlayerHandler(context);

        var result = await handler.UpdateMyProfileAsync(new UpdateMyProfileCommand(
            teamId,
            userId,
            null,
            null,
            null,
            null,
            true,
            DateTimeOffset.UtcNow.AddDays(2)),
            CancellationToken.None);

        Assert.Equal(ETeamPlayerOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_WhenHiatusLimitExceeded_ReturnsInvalidData()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.Users.Add(CreateUser(userId, "Player"));
        context.TeamMembers.Add(new TeamMember { Id = memberId, TeamId = teamId, UserId = userId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active });
        context.TeamMemberProfiles.Add(new TeamMemberProfile
        {
            TeamMemberId = memberId,
            IsOnHiatus = true,
            HiatusCountLast6Months = 1,
            LastHiatusStartedAt = DateTimeOffset.UtcNow.AddDays(-10)
        });
        AddSetting(context, teamId, TeamSettingKeys.MaxHiatusPerSemester, "1");
        await context.SaveChangesAsync();

        var handler = new TeamPlayerHandler(context);

        var result = await handler.UpdateMyProfileAsync(new UpdateMyProfileCommand(
            teamId,
            userId,
            null,
            null,
            null,
            null,
            true,
            DateTimeOffset.UtcNow.AddDays(20)),
            CancellationToken.None);

        Assert.Equal(ETeamPlayerOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_WhenStopHiatus_ClearsDates()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.Users.Add(CreateUser(userId, "Player"));
        context.TeamMembers.Add(new TeamMember { Id = memberId, TeamId = teamId, UserId = userId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active });
        context.TeamMemberProfiles.Add(new TeamMemberProfile
        {
            TeamMemberId = memberId,
            IsOnHiatus = true,
            HiatusStartedAt = DateTimeOffset.UtcNow.AddDays(-2),
            HiatusEndsAt = DateTimeOffset.UtcNow.AddDays(10)
        });
        await context.SaveChangesAsync();

        var handler = new TeamPlayerHandler(context);

        var result = await handler.UpdateMyProfileAsync(new UpdateMyProfileCommand(
            teamId,
            userId,
            null,
            null,
            null,
            null,
            false,
            null),
            CancellationToken.None);

        Assert.Equal(ETeamPlayerOperationStatus.Success, result.Status);
        var profile = await context.TeamMemberProfiles.FirstAsync(item => item.TeamMemberId == memberId);
        Assert.False(profile.IsOnHiatus);
        Assert.Null(profile.HiatusEndsAt);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_WhenHiatusEndsInvalid_ReturnsInvalidData()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var startedAt = new DateTimeOffset(2024, 1, 10, 0, 0, 0, TimeSpan.Zero);

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.Users.Add(CreateUser(userId, "Player"));
        context.TeamMembers.Add(new TeamMember { Id = memberId, TeamId = teamId, UserId = userId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active });
        context.TeamMemberProfiles.Add(new TeamMemberProfile
        {
            TeamMemberId = memberId,
            IsOnHiatus = true,
            HiatusStartedAt = startedAt,
            HiatusEndsAt = startedAt.AddDays(1)
        });
        AddSetting(context, teamId, TeamSettingKeys.MaxHiatusDays, "5");
        await context.SaveChangesAsync();

        var handler = new TeamPlayerHandler(context);

        var result = await handler.UpdateMyProfileAsync(new UpdateMyProfileCommand(
            teamId,
            userId,
            null,
            null,
            null,
            null,
            null,
            startedAt.AddDays(10)),
            CancellationToken.None);

        Assert.Equal(ETeamPlayerOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task UpdatePlayerAdminAsync_WhenInvalidData_ReturnsInvalidData()
    {
        await using var context = CreateContext();
        var handler = new TeamPlayerHandler(context);

        var result = await handler.UpdatePlayerAdminAsync(new UpdatePlayerAdminCommand(
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamPlayerOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task UpdatePlayerAdminAsync_WhenNotAdmin_ReturnsForbidden()
    {
        var teamId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = adminId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamPlayerHandler(context);

        var result = await handler.UpdatePlayerAdminAsync(new UpdatePlayerAdminCommand(
            teamId,
            userId,
            adminId,
            true,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamPlayerOperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task UpdatePlayerAdminAsync_WhenMemberNotFound_ReturnsNotFound()
    {
        var teamId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = adminId, Role = ETeamMemberRole.Admin, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamPlayerHandler(context);

        var result = await handler.UpdatePlayerAdminAsync(new UpdatePlayerAdminCommand(
            teamId,
            Guid.NewGuid(),
            adminId,
            true,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamPlayerOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task UpdatePlayerAdminAsync_WhenSuccess_UpdatesFeeExempt()
    {
        var teamId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.Users.Add(CreateUser(userId, "Player"));
        context.TeamMembers.AddRange(
            new TeamMember { TeamId = teamId, UserId = adminId, Role = ETeamMemberRole.Admin, Status = ETeamMemberStatus.Active },
            new TeamMember { Id = memberId, TeamId = teamId, UserId = userId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamPlayerHandler(context);

        var result = await handler.UpdatePlayerAdminAsync(new UpdatePlayerAdminCommand(
            teamId,
            userId,
            adminId,
            true,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamPlayerOperationStatus.Success, result.Status);
        var profile = await context.TeamMemberProfiles.FirstAsync(item => item.TeamMemberId == memberId);
        Assert.True(profile.IsFeeExempt);
    }

    [Fact]
    public async Task RemovePlayerAsync_WhenInvalidData_ReturnsInvalidData()
    {
        await using var context = CreateContext();
        var handler = new TeamPlayerHandler(context);

        var result = await handler.RemovePlayerAsync(new RemovePlayerCommand(
            Guid.Empty,
            Guid.Empty,
            Guid.Empty,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamPlayerOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task RemovePlayerAsync_WhenModeratorNotAllowed_ReturnsForbidden()
    {
        var teamId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.AddRange(
            new TeamMember { TeamId = teamId, UserId = moderatorId, Role = ETeamMemberRole.Moderator, Status = ETeamMemberStatus.Active },
            new TeamMember { TeamId = teamId, UserId = userId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active });
        AddSetting(context, teamId, TeamSettingKeys.ModeratorsCanRemovePlayers, "false");
        await context.SaveChangesAsync();

        var handler = new TeamPlayerHandler(context);

        var result = await handler.RemovePlayerAsync(new RemovePlayerCommand(
            teamId,
            userId,
            moderatorId,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamPlayerOperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task RemovePlayerAsync_WhenMemberNotFound_ReturnsNotFound()
    {
        var teamId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = adminId, Role = ETeamMemberRole.Admin, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamPlayerHandler(context);

        var result = await handler.RemovePlayerAsync(new RemovePlayerCommand(
            teamId,
            Guid.NewGuid(),
            adminId,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamPlayerOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task RemovePlayerAsync_WhenSuccess_RemovesPlayer()
    {
        var teamId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.AddRange(
            new TeamMember { TeamId = teamId, UserId = adminId, Role = ETeamMemberRole.Admin, Status = ETeamMemberStatus.Active },
            new TeamMember { TeamId = teamId, UserId = userId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamPlayerHandler(context);

        var result = await handler.RemovePlayerAsync(new RemovePlayerCommand(
            teamId,
            userId,
            adminId,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamPlayerOperationStatus.Success, result.Status);
        var member = await context.TeamMembers.FirstAsync(item => item.TeamId == teamId && item.UserId == userId);
        Assert.Equal(ETeamMemberStatus.Removed, member.Status);
    }

    private static Team CreateTeam(Guid teamId)
        => new()
        {
            Id = teamId,
            OwnerUserId = Guid.NewGuid(),
            Name = "Time",
            HomeFieldName = "Campo"
        };

    private static ApplicationUser CreateUser(Guid userId, string name)
        => new()
        {
            Id = userId,
            Email = $"{userId}@local",
            FullName = name,
            PhoneNumber = "11999999999"
        };

    private static void AddSetting(AppDbContext context, Guid teamId, string key, string value)
    {
        var settings = context.TeamSettings.FirstOrDefault(item => item.TeamId == teamId);
        if (settings is null)
        {
            settings = new TeamSettings { TeamId = teamId };
            context.TeamSettings.Add(settings);
        }

        context.TeamSettingEntries.Add(new TeamSettingEntry
        {
            TeamSettingsId = settings.Id,
            Key = key,
            Value = value,
            IsEnabled = true
        });
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }
}

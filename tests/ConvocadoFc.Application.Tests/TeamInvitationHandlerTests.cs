using ConvocadoFc.Application.Handlers.Modules.Teams.Implementations;
using ConvocadoFc.Application.Handlers.Modules.Teams.Models;
using ConvocadoFc.Domain.Models.Modules.Teams;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;
using ConvocadoFc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ConvocadoFc.Application.Tests;

public sealed class TeamInvitationHandlerTests
{
    [Fact]
    public async Task ListSentInvitesAsync_WhenNotModerator_ReturnsEmpty()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.ListSentInvitesAsync(new ListSentInvitesQuery(
            new PaginationQuery(),
            teamId,
            userId,
            false),
            CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
    }

    [Fact]
    public async Task ListSentInvitesAsync_WhenModerator_ReturnsOrdered()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.Add(CreateMember(teamId, userId, ETeamMemberRole.Moderator));
        context.TeamInvites.AddRange(
            CreateInvite(teamId, userId, new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero)),
            CreateInvite(teamId, userId, new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero)));
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.ListSentInvitesAsync(new ListSentInvitesQuery(
            new PaginationQuery { OrderBy = "createdAt" },
            teamId,
            userId,
            false),
            CancellationToken.None);

        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero), result.Items[0].CreatedAt);
    }

    [Fact]
    public async Task ListMyInvitesAsync_WhenUserHasInvites_ReturnsOnlyUserInvites()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamInvites.AddRange(
            CreateInvite(teamId, otherUserId, DateTimeOffset.UtcNow, targetUserId: userId),
            CreateInvite(teamId, otherUserId, DateTimeOffset.UtcNow, targetUserId: otherUserId));
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.ListMyInvitesAsync(new ListMyInvitesQuery(
            new PaginationQuery { Page = 1, PageSize = 10 },
            userId),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(userId, result.Items[0].TargetUserId);
    }

    [Fact]
    public async Task CreateInviteAsync_WhenInvalidData_ReturnsInvalidData()
    {
        await using var context = CreateContext();
        var handler = new TeamInvitationHandler(context);

        var result = await handler.CreateInviteAsync(new CreateInviteCommand(
            Guid.Empty,
            Guid.Empty,
            null,
            null,
            ETeamInviteChannel.Email,
            null,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task CreateInviteAsync_WhenTeamNotFound_ReturnsTeamNotFound()
    {
        await using var context = CreateContext();
        var handler = new TeamInvitationHandler(context);

        var result = await handler.CreateInviteAsync(new CreateInviteCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "user@local",
            ETeamInviteChannel.Email,
            null,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.TeamNotFound, result.Status);
    }

    [Fact]
    public async Task CreateInviteAsync_WhenModeratorNotAllowed_ReturnsForbidden()
    {
        var teamId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.Add(CreateMember(teamId, moderatorId, ETeamMemberRole.Moderator));
        AddSetting(context, teamId, TeamSettingKeys.ModeratorsCanInvite, "false");
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.CreateInviteAsync(new CreateInviteCommand(
            teamId,
            moderatorId,
            null,
            "user@local",
            ETeamInviteChannel.Email,
            null,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task CreateInviteAsync_WhenPlayerNotAllowed_ReturnsForbidden()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.Add(CreateMember(teamId, userId, ETeamMemberRole.User));
        AddSetting(context, teamId, TeamSettingKeys.AllowPlayersInvite, "false");
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.CreateInviteAsync(new CreateInviteCommand(
            teamId,
            userId,
            null,
            "user@local",
            ETeamInviteChannel.Email,
            null,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task CreateInviteAsync_WhenEmailTargetMissing_ReturnsInvalidData()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.Add(CreateMember(teamId, userId, ETeamMemberRole.Admin));
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.CreateInviteAsync(new CreateInviteCommand(
            teamId,
            userId,
            null,
            null,
            ETeamInviteChannel.Email,
            null,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task CreateInviteAsync_WhenMaxUsesInvalid_ReturnsInvalidData()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.Add(CreateMember(teamId, userId, ETeamMemberRole.Admin));
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.CreateInviteAsync(new CreateInviteCommand(
            teamId,
            userId,
            null,
            "user@local",
            ETeamInviteChannel.Email,
            0,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task CreateInviteAsync_WhenTargetUserNotFound_ReturnsUserNotFound()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.Add(CreateMember(teamId, userId, ETeamMemberRole.Admin));
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.CreateInviteAsync(new CreateInviteCommand(
            teamId,
            userId,
            Guid.NewGuid(),
            null,
            ETeamInviteChannel.Email,
            null,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.UserNotFound, result.Status);
    }

    [Fact]
    public async Task CreateInviteAsync_WhenSuccess_ReturnsInvite()
    {
        var teamId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.Users.Add(CreateUser(targetUserId));
        context.TeamMembers.Add(CreateMember(teamId, adminId, ETeamMemberRole.Admin));
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.CreateInviteAsync(new CreateInviteCommand(
            teamId,
            adminId,
            targetUserId,
            null,
            ETeamInviteChannel.Email,
            5,
            null,
            " msg ",
            false),
            CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.Success, result.Status);
        Assert.NotNull(result.Invite);
        Assert.True(result.Invite!.IsPreApproved);
        Assert.Equal("msg", result.Invite.Message);
    }

    [Fact]
    public async Task AcceptInviteAsync_WhenInvalidData_ReturnsInvalidData()
    {
        await using var context = CreateContext();
        var handler = new TeamInvitationHandler(context);

        var result = await handler.AcceptInviteAsync(new AcceptInviteCommand(Guid.Empty, Guid.Empty), CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task AcceptInviteAsync_WhenNotFound_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var handler = new TeamInvitationHandler(context);

        var result = await handler.AcceptInviteAsync(new AcceptInviteCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task AcceptInviteAsync_WhenTargetMismatch_ReturnsForbidden()
    {
        var teamId = Guid.NewGuid();
        var inviteId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamInvites.Add(new TeamInvite
        {
            Id = inviteId,
            TeamId = teamId,
            CreatedByUserId = Guid.NewGuid(),
            TargetUserId = targetUserId,
            Status = ETeamInviteStatus.Pending
        });
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.AcceptInviteAsync(new AcceptInviteCommand(inviteId, Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task AcceptInviteAsync_WhenAlreadyProcessed_ReturnsAlreadyProcessed()
    {
        var teamId = Guid.NewGuid();
        var inviteId = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamInvites.Add(new TeamInvite
        {
            Id = inviteId,
            TeamId = teamId,
            CreatedByUserId = Guid.NewGuid(),
            Status = ETeamInviteStatus.Accepted
        });
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.AcceptInviteAsync(new AcceptInviteCommand(inviteId, Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.AlreadyProcessed, result.Status);
        Assert.NotNull(result.Invite);
    }

    [Fact]
    public async Task AcceptInviteAsync_WhenExpired_ReturnsInviteExpired()
    {
        var teamId = Guid.NewGuid();
        var inviteId = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamInvites.Add(new TeamInvite
        {
            Id = inviteId,
            TeamId = teamId,
            CreatedByUserId = Guid.NewGuid(),
            Status = ETeamInviteStatus.Pending,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1)
        });
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.AcceptInviteAsync(new AcceptInviteCommand(inviteId, Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.InviteExpired, result.Status);
        var invite = await context.TeamInvites.FirstAsync(item => item.Id == inviteId);
        Assert.Equal(ETeamInviteStatus.Expired, invite.Status);
    }

    [Fact]
    public async Task AcceptInviteAsync_WhenMaxUsesReached_ReturnsMaxUsesReached()
    {
        var teamId = Guid.NewGuid();
        var inviteId = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamInvites.Add(new TeamInvite
        {
            Id = inviteId,
            TeamId = teamId,
            CreatedByUserId = Guid.NewGuid(),
            Status = ETeamInviteStatus.Pending,
            MaxUses = 1,
            UseCount = 1
        });
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.AcceptInviteAsync(new AcceptInviteCommand(inviteId, Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.MaxUsesReached, result.Status);
    }

    [Fact]
    public async Task AcceptInviteAsync_WhenAlreadyMember_ReturnsAlreadyMember()
    {
        var teamId = Guid.NewGuid();
        var inviteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamInvites.Add(new TeamInvite
        {
            Id = inviteId,
            TeamId = teamId,
            CreatedByUserId = Guid.NewGuid(),
            Status = ETeamInviteStatus.Pending
        });
        context.TeamMembers.Add(CreateMember(teamId, userId, ETeamMemberRole.User));
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.AcceptInviteAsync(new AcceptInviteCommand(inviteId, userId), CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.AlreadyMember, result.Status);
    }

    [Fact]
    public async Task AcceptInviteAsync_WhenAutoApprovedCapacityExceeded_ReturnsInvalidData()
    {
        var teamId = Guid.NewGuid();
        var inviteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamInvites.Add(new TeamInvite
        {
            Id = inviteId,
            TeamId = teamId,
            CreatedByUserId = Guid.NewGuid(),
            Status = ETeamInviteStatus.Pending,
            IsPreApproved = true
        });
        context.TeamMembers.Add(CreateMember(teamId, Guid.NewGuid(), ETeamMemberRole.User));
        AddSetting(context, teamId, TeamSettingKeys.MaxPlayers, "1");
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.AcceptInviteAsync(new AcceptInviteCommand(inviteId, userId), CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task AcceptInviteAsync_WhenAutoApproved_AddsMember()
    {
        var teamId = Guid.NewGuid();
        var inviteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamInvites.Add(new TeamInvite
        {
            Id = inviteId,
            TeamId = teamId,
            CreatedByUserId = Guid.NewGuid(),
            Status = ETeamInviteStatus.Pending,
            IsPreApproved = true
        });
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.AcceptInviteAsync(new AcceptInviteCommand(inviteId, userId), CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.Success, result.Status);
        Assert.Single(context.TeamMembers);
        Assert.Single(context.TeamJoinRequests);
    }

    [Fact]
    public async Task AcceptInviteAsync_WhenManualApproval_CreatesPendingRequest()
    {
        var teamId = Guid.NewGuid();
        var inviteId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamInvites.Add(new TeamInvite
        {
            Id = inviteId,
            TeamId = teamId,
            CreatedByUserId = Guid.NewGuid(),
            Status = ETeamInviteStatus.Pending,
            IsPreApproved = false,
            Channel = ETeamInviteChannel.QrCode
        });
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.AcceptInviteAsync(new AcceptInviteCommand(inviteId, userId), CancellationToken.None);

        Assert.Equal(ETeamInviteOperationStatus.Success, result.Status);
        var request = await context.TeamJoinRequests.FirstAsync();
        Assert.Equal(ETeamJoinRequestStatus.Pending, request.Status);
        Assert.Equal(ETeamJoinRequestSource.QrCode, request.Source);
        Assert.Empty(context.TeamMembers);
    }

    [Fact]
    public async Task ListJoinRequestsAsync_WhenNotModerator_ReturnsEmpty()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.ListJoinRequestsAsync(new ListJoinRequestsQuery(
            new PaginationQuery(),
            teamId,
            userId,
            null,
            false),
            CancellationToken.None);

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task ListJoinRequestsAsync_WhenStatusFiltered_ReturnsOnlyStatus()
    {
        var teamId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.Add(CreateMember(teamId, adminId, ETeamMemberRole.Admin));
        context.TeamJoinRequests.AddRange(
            new TeamJoinRequest { TeamId = teamId, UserId = Guid.NewGuid(), Status = ETeamJoinRequestStatus.Pending },
            new TeamJoinRequest { TeamId = teamId, UserId = Guid.NewGuid(), Status = ETeamJoinRequestStatus.Approved });
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.ListJoinRequestsAsync(new ListJoinRequestsQuery(
            new PaginationQuery { OrderBy = "status" },
            teamId,
            adminId,
            ETeamJoinRequestStatus.Pending,
            false),
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(ETeamJoinRequestStatus.Pending, result.Items[0].Status);
    }

    [Fact]
    public async Task CreateJoinRequestAsync_WhenInvalidData_ReturnsInvalidData()
    {
        await using var context = CreateContext();
        var handler = new TeamInvitationHandler(context);

        var result = await handler.CreateJoinRequestAsync(new CreateJoinRequestCommand(
            Guid.Empty,
            Guid.Empty,
            null,
            null,
            ETeamJoinRequestSource.ProximitySearch),
            CancellationToken.None);

        Assert.Equal(ETeamJoinRequestOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task CreateJoinRequestAsync_WhenTeamNotFound_ReturnsTeamNotFound()
    {
        await using var context = CreateContext();
        var handler = new TeamInvitationHandler(context);

        var result = await handler.CreateJoinRequestAsync(new CreateJoinRequestCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            ETeamJoinRequestSource.ProximitySearch),
            CancellationToken.None);

        Assert.Equal(ETeamJoinRequestOperationStatus.TeamNotFound, result.Status);
    }

    [Fact]
    public async Task CreateJoinRequestAsync_WhenUserNotFound_ReturnsUserNotFound()
    {
        var teamId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.CreateJoinRequestAsync(new CreateJoinRequestCommand(
            teamId,
            Guid.NewGuid(),
            null,
            null,
            ETeamJoinRequestSource.ProximitySearch),
            CancellationToken.None);

        Assert.Equal(ETeamJoinRequestOperationStatus.UserNotFound, result.Status);
    }

    [Fact]
    public async Task CreateJoinRequestAsync_WhenAlreadyMember_ReturnsAlreadyMember()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.Users.Add(CreateUser(userId));
        context.TeamMembers.Add(CreateMember(teamId, userId, ETeamMemberRole.User));
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.CreateJoinRequestAsync(new CreateJoinRequestCommand(
            teamId,
            userId,
            null,
            null,
            ETeamJoinRequestSource.ProximitySearch),
            CancellationToken.None);

        Assert.Equal(ETeamJoinRequestOperationStatus.AlreadyMember, result.Status);
    }

    [Fact]
    public async Task CreateJoinRequestAsync_WhenPendingExists_ReturnsAlreadyProcessed()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.Users.Add(CreateUser(userId));
        context.TeamJoinRequests.Add(new TeamJoinRequest
        {
            TeamId = teamId,
            UserId = userId,
            Status = ETeamJoinRequestStatus.Pending
        });
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.CreateJoinRequestAsync(new CreateJoinRequestCommand(
            teamId,
            userId,
            null,
            null,
            ETeamJoinRequestSource.ProximitySearch),
            CancellationToken.None);

        Assert.Equal(ETeamJoinRequestOperationStatus.AlreadyProcessed, result.Status);
    }

    [Fact]
    public async Task CreateJoinRequestAsync_WhenInviteMissing_ReturnsInviteNotFound()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.Users.Add(CreateUser(userId));
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.CreateJoinRequestAsync(new CreateJoinRequestCommand(
            teamId,
            userId,
            null,
            Guid.NewGuid(),
            ETeamJoinRequestSource.ProximitySearch),
            CancellationToken.None);

        Assert.Equal(ETeamJoinRequestOperationStatus.InviteNotFound, result.Status);
    }

    [Fact]
    public async Task CreateJoinRequestAsync_WhenInviteExpired_ReturnsInviteExpired()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var inviteId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.Users.Add(CreateUser(userId));
        context.TeamInvites.Add(new TeamInvite
        {
            Id = inviteId,
            TeamId = teamId,
            CreatedByUserId = Guid.NewGuid(),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            Status = ETeamInviteStatus.Pending
        });
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.CreateJoinRequestAsync(new CreateJoinRequestCommand(
            teamId,
            userId,
            null,
            inviteId,
            ETeamJoinRequestSource.ProximitySearch),
            CancellationToken.None);

        Assert.Equal(ETeamJoinRequestOperationStatus.InviteExpired, result.Status);
    }

    [Fact]
    public async Task CreateJoinRequestAsync_WhenInviteMaxUsesReached_ReturnsMaxUsesReached()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var inviteId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.Users.Add(CreateUser(userId));
        context.TeamInvites.Add(new TeamInvite
        {
            Id = inviteId,
            TeamId = teamId,
            CreatedByUserId = Guid.NewGuid(),
            MaxUses = 1,
            UseCount = 1,
            Status = ETeamInviteStatus.Pending
        });
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.CreateJoinRequestAsync(new CreateJoinRequestCommand(
            teamId,
            userId,
            null,
            inviteId,
            ETeamJoinRequestSource.ProximitySearch),
            CancellationToken.None);

        Assert.Equal(ETeamJoinRequestOperationStatus.MaxUsesReached, result.Status);
    }

    [Fact]
    public async Task CreateJoinRequestAsync_WhenAutoApproved_AddsMember()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var inviteId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.Users.Add(CreateUser(userId));
        context.TeamInvites.Add(new TeamInvite
        {
            Id = inviteId,
            TeamId = teamId,
            CreatedByUserId = Guid.NewGuid(),
            IsPreApproved = true,
            Status = ETeamInviteStatus.Pending
        });
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.CreateJoinRequestAsync(new CreateJoinRequestCommand(
            teamId,
            userId,
            " msg ",
            inviteId,
            ETeamJoinRequestSource.ProximitySearch),
            CancellationToken.None);

        Assert.Equal(ETeamJoinRequestOperationStatus.Success, result.Status);
        Assert.Single(context.TeamMembers);
        Assert.Equal("msg", result.Request!.Message);
    }

    [Fact]
    public async Task ReviewJoinRequestAsync_WhenInvalidData_ReturnsInvalidData()
    {
        await using var context = CreateContext();
        var handler = new TeamInvitationHandler(context);

        var result = await handler.ReviewJoinRequestAsync(new ReviewJoinRequestCommand(
            Guid.Empty,
            Guid.Empty,
            true,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamJoinRequestOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task ReviewJoinRequestAsync_WhenNotFound_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var handler = new TeamInvitationHandler(context);

        var result = await handler.ReviewJoinRequestAsync(new ReviewJoinRequestCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            true,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamJoinRequestOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task ReviewJoinRequestAsync_WhenNoMember_ReturnsForbidden()
    {
        var requestId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamJoinRequests.Add(new TeamJoinRequest { Id = requestId, TeamId = teamId, UserId = Guid.NewGuid() });
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.ReviewJoinRequestAsync(new ReviewJoinRequestCommand(
            requestId,
            Guid.NewGuid(),
            true,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamJoinRequestOperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task ReviewJoinRequestAsync_WhenModeratorNotAllowed_ReturnsForbidden()
    {
        var requestId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamJoinRequests.Add(new TeamJoinRequest { Id = requestId, TeamId = teamId, UserId = Guid.NewGuid() });
        context.TeamMembers.Add(CreateMember(teamId, moderatorId, ETeamMemberRole.Moderator));
        AddSetting(context, teamId, TeamSettingKeys.ModeratorsCanApproveRequests, "false");
        AddSetting(context, teamId, TeamSettingKeys.ModeratorsCanRejectRequests, "false");
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.ReviewJoinRequestAsync(new ReviewJoinRequestCommand(
            requestId,
            moderatorId,
            true,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamJoinRequestOperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task ReviewJoinRequestAsync_WhenAlreadyProcessed_ReturnsAlreadyProcessed()
    {
        var requestId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamJoinRequests.Add(new TeamJoinRequest
        {
            Id = requestId,
            TeamId = teamId,
            UserId = Guid.NewGuid(),
            Status = ETeamJoinRequestStatus.Approved
        });
        context.TeamMembers.Add(CreateMember(teamId, adminId, ETeamMemberRole.Admin));
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.ReviewJoinRequestAsync(new ReviewJoinRequestCommand(
            requestId,
            adminId,
            true,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamJoinRequestOperationStatus.AlreadyProcessed, result.Status);
    }

    [Fact]
    public async Task ReviewJoinRequestAsync_WhenApproveCapacityExceeded_ReturnsInvalidData()
    {
        var requestId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamJoinRequests.Add(new TeamJoinRequest { Id = requestId, TeamId = teamId, UserId = Guid.NewGuid() });
        context.TeamMembers.Add(CreateMember(teamId, adminId, ETeamMemberRole.Admin));
        context.TeamMembers.Add(CreateMember(teamId, Guid.NewGuid(), ETeamMemberRole.User));
        AddSetting(context, teamId, TeamSettingKeys.MaxPlayers, "1");
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.ReviewJoinRequestAsync(new ReviewJoinRequestCommand(
            requestId,
            adminId,
            true,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamJoinRequestOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task ReviewJoinRequestAsync_WhenApprove_AddsMember()
    {
        var requestId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamJoinRequests.Add(new TeamJoinRequest { Id = requestId, TeamId = teamId, UserId = userId });
        context.TeamMembers.Add(CreateMember(teamId, adminId, ETeamMemberRole.Admin));
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.ReviewJoinRequestAsync(new ReviewJoinRequestCommand(
            requestId,
            adminId,
            true,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamJoinRequestOperationStatus.Success, result.Status);
        Assert.Single(context.TeamMembers.Where(member => member.UserId == userId));
    }

    [Fact]
    public async Task ReviewJoinRequestAsync_WhenReject_UpdatesStatus()
    {
        var requestId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamJoinRequests.Add(new TeamJoinRequest { Id = requestId, TeamId = teamId, UserId = Guid.NewGuid() });
        context.TeamMembers.Add(CreateMember(teamId, adminId, ETeamMemberRole.Admin));
        await context.SaveChangesAsync();

        var handler = new TeamInvitationHandler(context);

        var result = await handler.ReviewJoinRequestAsync(new ReviewJoinRequestCommand(
            requestId,
            adminId,
            false,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamJoinRequestOperationStatus.Success, result.Status);
        var request = await context.TeamJoinRequests.FirstAsync(item => item.Id == requestId);
        Assert.Equal(ETeamJoinRequestStatus.Rejected, request.Status);
    }

    private static Team CreateTeam(Guid teamId)
        => new()
        {
            Id = teamId,
            OwnerUserId = Guid.NewGuid(),
            Name = "Time",
            HomeFieldName = "Campo"
        };

    private static ApplicationUser CreateUser(Guid userId)
        => new()
        {
            Id = userId,
            Email = $"{userId}@local",
            FullName = "User",
            PhoneNumber = "11999999999"
        };

    private static TeamMember CreateMember(Guid teamId, Guid userId, ETeamMemberRole role)
        => new()
        {
            TeamId = teamId,
            UserId = userId,
            Role = role,
            Status = ETeamMemberStatus.Active
        };

    private static TeamInvite CreateInvite(Guid teamId, Guid createdByUserId, DateTimeOffset createdAt, Guid? targetUserId = null)
        => new()
        {
            TeamId = teamId,
            CreatedByUserId = createdByUserId,
            TargetUserId = targetUserId,
            Status = ETeamInviteStatus.Pending,
            CreatedAt = createdAt,
            Token = Guid.NewGuid().ToString("N")
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

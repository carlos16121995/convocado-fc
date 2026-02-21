using ConvocadoFc.Application.Abstractions;
using ConvocadoFc.Application.Handlers.Modules.Teams.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Teams.Models;
using ConvocadoFc.Domain.Models.Modules.Teams;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;

using Microsoft.EntityFrameworkCore;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Implementations;

public sealed class TeamInvitationHandler(IApplicationDbContext dbContext) : ITeamInvitationHandler
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<PaginatedResult<TeamInviteDto>> ListSentInvitesAsync(ListSentInvitesQuery query, CancellationToken cancellationToken)
    {
        var member = await GetMemberAsync(query.TeamId, query.CurrentUserId, cancellationToken);
        if (!query.IsSystemAdmin && (member is null || !IsModeratorOrAdmin(member)))
        {
            return new PaginatedResult<TeamInviteDto>();
        }

        var invitesQuery = _dbContext.Query<TeamInvite>()
            .Where(invite => invite.TeamId == query.TeamId);

        invitesQuery = ApplyInviteOrdering(invitesQuery, query.Pagination.OrderBy);

        var totalItems = await invitesQuery.CountAsync(cancellationToken);
        var page = query.Pagination.Page <= 0 ? 1 : query.Pagination.Page;
        var pageSize = query.Pagination.PageSize <= 0 ? 20 : query.Pagination.PageSize;

        var items = await invitesQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(invite => MapToDto(invite))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<TeamInviteDto>
        {
            TotalItems = totalItems,
            Items = items
        };
    }

    public async Task<PaginatedResult<TeamInviteDto>> ListMyInvitesAsync(ListMyInvitesQuery query, CancellationToken cancellationToken)
    {
        var invitesQuery = _dbContext.Query<TeamInvite>()
            .Where(invite => invite.TargetUserId == query.UserId);

        invitesQuery = ApplyInviteOrdering(invitesQuery, query.Pagination.OrderBy);

        var totalItems = await invitesQuery.CountAsync(cancellationToken);
        var page = query.Pagination.Page <= 0 ? 1 : query.Pagination.Page;
        var pageSize = query.Pagination.PageSize <= 0 ? 20 : query.Pagination.PageSize;

        var items = await invitesQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(invite => MapToDto(invite))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<TeamInviteDto>
        {
            TotalItems = totalItems,
            Items = items
        };
    }

    public async Task<TeamInviteOperationResult> CreateInviteAsync(CreateInviteCommand command, CancellationToken cancellationToken)
    {
        if (command.TeamId == Guid.Empty || command.CreatedByUserId == Guid.Empty)
        {
            return new TeamInviteOperationResult(ETeamInviteOperationStatus.InvalidData, null);
        }

        var teamExists = await _dbContext.Query<Team>()
            .AnyAsync(team => team.Id == command.TeamId, cancellationToken);

        if (!teamExists)
        {
            return new TeamInviteOperationResult(ETeamInviteOperationStatus.TeamNotFound, null);
        }

        var member = await GetMemberAsync(command.TeamId, command.CreatedByUserId, cancellationToken);
        if (!command.IsSystemAdmin)
        {
            if (member is null)
            {
                return new TeamInviteOperationResult(ETeamInviteOperationStatus.Forbidden, null);
            }

            var settings = await GetTeamSettingsAsync(command.TeamId, cancellationToken);
            if (member.Role == ETeamMemberRole.Moderator && !GetBoolSetting(settings, TeamSettingKeys.ModeratorsCanInvite))
            {
                return new TeamInviteOperationResult(ETeamInviteOperationStatus.Forbidden, null);
            }

            if (member.Role == ETeamMemberRole.User && !GetBoolSetting(settings, TeamSettingKeys.AllowPlayersInvite))
            {
                return new TeamInviteOperationResult(ETeamInviteOperationStatus.Forbidden, null);
            }
        }

        if (command.Channel == ETeamInviteChannel.Email && string.IsNullOrWhiteSpace(command.TargetEmail) && !command.TargetUserId.HasValue)
        {
            return new TeamInviteOperationResult(ETeamInviteOperationStatus.InvalidData, null);
        }

        if (command.MaxUses.HasValue && command.MaxUses.Value <= 0)
        {
            return new TeamInviteOperationResult(ETeamInviteOperationStatus.InvalidData, null);
        }

        if (command.TargetUserId.HasValue)
        {
            var targetExists = await _dbContext.Query<ApplicationUser>()
                .AnyAsync(user => user.Id == command.TargetUserId.Value, cancellationToken);

            if (!targetExists)
            {
                return new TeamInviteOperationResult(ETeamInviteOperationStatus.UserNotFound, null);
            }
        }

        var invite = new TeamInvite
        {
            Id = Guid.NewGuid(),
            TeamId = command.TeamId,
            CreatedByUserId = command.CreatedByUserId,
            TargetUserId = command.TargetUserId,
            TargetEmail = NormalizeNullable(command.TargetEmail),
            Token = Guid.NewGuid().ToString("N"),
            Channel = command.Channel,
            Status = ETeamInviteStatus.Pending,
            IsPreApproved = command.IsSystemAdmin || member?.Role == ETeamMemberRole.Admin,
            MaxUses = command.MaxUses,
            UseCount = 0,
            Message = NormalizeNullable(command.Message),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = command.ExpiresAt
        };

        await _dbContext.AddAsync(invite, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TeamInviteOperationResult(ETeamInviteOperationStatus.Success, MapToDto(invite));
    }

    public async Task<TeamInviteOperationResult> AcceptInviteAsync(AcceptInviteCommand command, CancellationToken cancellationToken)
    {
        if (command.InviteId == Guid.Empty || command.UserId == Guid.Empty)
        {
            return new TeamInviteOperationResult(ETeamInviteOperationStatus.InvalidData, null);
        }

        var invite = await _dbContext.Track<TeamInvite>()
            .FirstOrDefaultAsync(existing => existing.Id == command.InviteId, cancellationToken);

        if (invite is null)
        {
            return new TeamInviteOperationResult(ETeamInviteOperationStatus.NotFound, null);
        }

        if (invite.TargetUserId.HasValue && invite.TargetUserId.Value != command.UserId)
        {
            return new TeamInviteOperationResult(ETeamInviteOperationStatus.Forbidden, null);
        }

        if (invite.Status != ETeamInviteStatus.Pending)
        {
            return new TeamInviteOperationResult(ETeamInviteOperationStatus.AlreadyProcessed, MapToDto(invite));
        }

        if (invite.ExpiresAt.HasValue && invite.ExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            invite.Status = ETeamInviteStatus.Expired;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return new TeamInviteOperationResult(ETeamInviteOperationStatus.InviteExpired, MapToDto(invite));
        }

        if (invite.MaxUses.HasValue && invite.UseCount >= invite.MaxUses.Value)
        {
            return new TeamInviteOperationResult(ETeamInviteOperationStatus.MaxUsesReached, MapToDto(invite));
        }

        var memberExists = await _dbContext.Query<TeamMember>()
            .AnyAsync(member => member.TeamId == invite.TeamId && member.UserId == command.UserId && member.Status == ETeamMemberStatus.Active, cancellationToken);

        if (memberExists)
        {
            return new TeamInviteOperationResult(ETeamInviteOperationStatus.AlreadyMember, MapToDto(invite));
        }

        var autoApproved = invite.IsPreApproved;
        var source = ResolveSource(invite);
        var joinStatus = autoApproved ? ETeamJoinRequestStatus.Approved : ETeamJoinRequestStatus.Pending;

        if (autoApproved)
        {
            var canAdd = await CanAddMemberAsync(invite.TeamId, cancellationToken);
            if (!canAdd)
            {
                return new TeamInviteOperationResult(ETeamInviteOperationStatus.InvalidData, MapToDto(invite));
            }
        }

        var joinRequest = new TeamJoinRequest
        {
            Id = Guid.NewGuid(),
            TeamId = invite.TeamId,
            UserId = command.UserId,
            InviteId = invite.Id,
            Status = joinStatus,
            Source = autoApproved ? ETeamJoinRequestSource.AdminLink : source,
            IsAutoApproved = autoApproved,
            Message = invite.Message,
            RequestedAt = DateTimeOffset.UtcNow,
            ReviewedAt = autoApproved ? DateTimeOffset.UtcNow : null,
            ReviewedByUserId = autoApproved ? invite.CreatedByUserId : null
        };

        await _dbContext.ExecuteInTransactionAsync(async ct =>
        {
            await _dbContext.AddAsync(joinRequest, ct);

            if (autoApproved)
            {
                var member = new TeamMember
                {
                    Id = Guid.NewGuid(),
                    TeamId = invite.TeamId,
                    UserId = command.UserId,
                    AddedByUserId = invite.CreatedByUserId,
                    Role = ETeamMemberRole.User,
                    Status = ETeamMemberStatus.Active,
                    JoinedAt = DateTimeOffset.UtcNow
                };

                await _dbContext.AddAsync(member, ct);
            }

            invite.UseCount += 1;
            invite.Status = ETeamInviteStatus.Accepted;
            invite.AcceptedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(ct);
        }, cancellationToken);

        return new TeamInviteOperationResult(ETeamInviteOperationStatus.Success, MapToDto(invite));
    }

    public async Task<PaginatedResult<TeamJoinRequestDto>> ListJoinRequestsAsync(ListJoinRequestsQuery query, CancellationToken cancellationToken)
    {
        var member = await GetMemberAsync(query.TeamId, query.CurrentUserId, cancellationToken);
        if (!query.IsSystemAdmin && (member is null || !IsModeratorOrAdmin(member)))
        {
            return new PaginatedResult<TeamJoinRequestDto>();
        }

        var requestsQuery = _dbContext.Query<TeamJoinRequest>()
            .Where(request => request.TeamId == query.TeamId);

        if (query.Status.HasValue)
        {
            requestsQuery = requestsQuery.Where(request => request.Status == query.Status.Value);
        }

        requestsQuery = ApplyJoinRequestOrdering(requestsQuery, query.Pagination.OrderBy);

        var totalItems = await requestsQuery.CountAsync(cancellationToken);
        var page = query.Pagination.Page <= 0 ? 1 : query.Pagination.Page;
        var pageSize = query.Pagination.PageSize <= 0 ? 20 : query.Pagination.PageSize;

        var items = await requestsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(request => MapToDto(request))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<TeamJoinRequestDto>
        {
            TotalItems = totalItems,
            Items = items
        };
    }

    public async Task<TeamJoinRequestOperationResult> CreateJoinRequestAsync(CreateJoinRequestCommand command, CancellationToken cancellationToken)
    {
        if (command.TeamId == Guid.Empty || command.UserId == Guid.Empty)
        {
            return new TeamJoinRequestOperationResult(ETeamJoinRequestOperationStatus.InvalidData, null);
        }

        var teamExists = await _dbContext.Query<Team>()
            .AnyAsync(team => team.Id == command.TeamId, cancellationToken);

        if (!teamExists)
        {
            return new TeamJoinRequestOperationResult(ETeamJoinRequestOperationStatus.TeamNotFound, null);
        }

        var userExists = await _dbContext.Query<ApplicationUser>()
            .AnyAsync(user => user.Id == command.UserId, cancellationToken);

        if (!userExists)
        {
            return new TeamJoinRequestOperationResult(ETeamJoinRequestOperationStatus.UserNotFound, null);
        }

        var alreadyMember = await _dbContext.Query<TeamMember>()
            .AnyAsync(member => member.TeamId == command.TeamId && member.UserId == command.UserId && member.Status == ETeamMemberStatus.Active, cancellationToken);

        if (alreadyMember)
        {
            return new TeamJoinRequestOperationResult(ETeamJoinRequestOperationStatus.AlreadyMember, null);
        }

        var pendingRequestExists = await _dbContext.Query<TeamJoinRequest>()
            .AnyAsync(request => request.TeamId == command.TeamId
                                 && request.UserId == command.UserId
                                 && request.Status == ETeamJoinRequestStatus.Pending, cancellationToken);

        if (pendingRequestExists)
        {
            return new TeamJoinRequestOperationResult(ETeamJoinRequestOperationStatus.AlreadyProcessed, null);
        }

        TeamInvite? invite = null;
        if (command.InviteId.HasValue)
        {
            invite = await _dbContext.Track<TeamInvite>()
                .FirstOrDefaultAsync(existing => existing.Id == command.InviteId.Value, cancellationToken);

            if (invite is null)
            {
                return new TeamJoinRequestOperationResult(ETeamJoinRequestOperationStatus.InviteNotFound, null);
            }

            if (invite.ExpiresAt.HasValue && invite.ExpiresAt.Value <= DateTimeOffset.UtcNow)
            {
                invite.Status = ETeamInviteStatus.Expired;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return new TeamJoinRequestOperationResult(ETeamJoinRequestOperationStatus.InviteExpired, null);
            }

            if (invite.MaxUses.HasValue && invite.UseCount >= invite.MaxUses.Value)
            {
                return new TeamJoinRequestOperationResult(ETeamJoinRequestOperationStatus.MaxUsesReached, null);
            }
        }

        var autoApproved = invite?.IsPreApproved == true;
        var source = invite is null ? command.Source : ResolveSource(invite);
        if (autoApproved)
        {
            source = ETeamJoinRequestSource.AdminLink;
        }

        var joinRequest = new TeamJoinRequest
        {
            Id = Guid.NewGuid(),
            TeamId = command.TeamId,
            UserId = command.UserId,
            InviteId = invite?.Id,
            Status = autoApproved ? ETeamJoinRequestStatus.Approved : ETeamJoinRequestStatus.Pending,
            Source = source,
            IsAutoApproved = autoApproved,
            Message = NormalizeNullable(command.Message),
            RequestedAt = DateTimeOffset.UtcNow,
            ReviewedAt = autoApproved ? DateTimeOffset.UtcNow : null,
            ReviewedByUserId = autoApproved ? invite?.CreatedByUserId : null
        };

        await _dbContext.ExecuteInTransactionAsync(async ct =>
        {
            await _dbContext.AddAsync(joinRequest, ct);

            if (autoApproved)
            {
                var member = new TeamMember
                {
                    Id = Guid.NewGuid(),
                    TeamId = command.TeamId,
                    UserId = command.UserId,
                    AddedByUserId = invite?.CreatedByUserId,
                    Role = ETeamMemberRole.User,
                    Status = ETeamMemberStatus.Active,
                    JoinedAt = DateTimeOffset.UtcNow
                };

                await _dbContext.AddAsync(member, ct);
            }

            if (invite is not null)
            {
                invite.UseCount += 1;
                invite.Status = ETeamInviteStatus.Accepted;
                invite.AcceptedAt = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync(ct);
        }, cancellationToken);

        return new TeamJoinRequestOperationResult(ETeamJoinRequestOperationStatus.Success, MapToDto(joinRequest));
    }

    public async Task<TeamJoinRequestOperationResult> ReviewJoinRequestAsync(ReviewJoinRequestCommand command, CancellationToken cancellationToken)
    {
        if (command.RequestId == Guid.Empty || command.ReviewedByUserId == Guid.Empty)
        {
            return new TeamJoinRequestOperationResult(ETeamJoinRequestOperationStatus.InvalidData, null);
        }

        var request = await _dbContext.Track<TeamJoinRequest>()
            .FirstOrDefaultAsync(existing => existing.Id == command.RequestId, cancellationToken);

        if (request is null)
        {
            return new TeamJoinRequestOperationResult(ETeamJoinRequestOperationStatus.NotFound, null);
        }

        var member = await GetMemberAsync(request.TeamId, command.ReviewedByUserId, cancellationToken);
        if (!command.IsSystemAdmin)
        {
            if (member is null)
            {
                return new TeamJoinRequestOperationResult(ETeamJoinRequestOperationStatus.Forbidden, null);
            }

            if (member.Role == ETeamMemberRole.Moderator)
            {
                var settings = await GetTeamSettingsAsync(request.TeamId, cancellationToken);
                var allowed = command.Approve
                    ? GetBoolSetting(settings, TeamSettingKeys.ModeratorsCanApproveRequests)
                    : GetBoolSetting(settings, TeamSettingKeys.ModeratorsCanRejectRequests);

                if (!allowed)
                {
                    return new TeamJoinRequestOperationResult(ETeamJoinRequestOperationStatus.Forbidden, null);
                }
            }
            else if (member.Role != ETeamMemberRole.Admin)
            {
                return new TeamJoinRequestOperationResult(ETeamJoinRequestOperationStatus.Forbidden, null);
            }
        }

        if (request.Status != ETeamJoinRequestStatus.Pending)
        {
            return new TeamJoinRequestOperationResult(ETeamJoinRequestOperationStatus.AlreadyProcessed, MapToDto(request));
        }

        var canAddMember = command.Approve && await CanAddMemberAsync(request.TeamId, cancellationToken);
        if (command.Approve && !canAddMember)
        {
            return new TeamJoinRequestOperationResult(ETeamJoinRequestOperationStatus.InvalidData, null);
        }

        await _dbContext.ExecuteInTransactionAsync(async ct =>
        {
            if (command.Approve)
            {
                request.Status = ETeamJoinRequestStatus.Approved;
                request.ReviewedAt = DateTimeOffset.UtcNow;
                request.ReviewedByUserId = command.ReviewedByUserId;

                var memberExists = await _dbContext.Query<TeamMember>()
                    .AnyAsync(existing => existing.TeamId == request.TeamId
                                          && existing.UserId == request.UserId
                                          && existing.Status == ETeamMemberStatus.Active, ct);

                if (!memberExists)
                {
                    var memberToAdd = new TeamMember
                    {
                        Id = Guid.NewGuid(),
                        TeamId = request.TeamId,
                        UserId = request.UserId,
                        AddedByUserId = command.ReviewedByUserId,
                        Role = ETeamMemberRole.User,
                        Status = ETeamMemberStatus.Active,
                        JoinedAt = DateTimeOffset.UtcNow
                    };

                    await _dbContext.AddAsync(memberToAdd, ct);
                }
            }
            else
            {
                request.Status = ETeamJoinRequestStatus.Rejected;
                request.ReviewedAt = DateTimeOffset.UtcNow;
                request.ReviewedByUserId = command.ReviewedByUserId;
            }

            await _dbContext.SaveChangesAsync(ct);
        }, cancellationToken);

        return new TeamJoinRequestOperationResult(ETeamJoinRequestOperationStatus.Success, MapToDto(request));
    }

    private async Task<TeamMember?> GetMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken)
        => await _dbContext.Query<TeamMember>()
            .FirstOrDefaultAsync(member => member.TeamId == teamId && member.UserId == userId, cancellationToken);

    private static bool IsModeratorOrAdmin(TeamMember member)
        => member.Role == ETeamMemberRole.Admin || member.Role == ETeamMemberRole.Moderator;

    private static IQueryable<TeamInvite> ApplyInviteOrdering(IQueryable<TeamInvite> query, string? orderBy)
        => orderBy?.Trim().ToLowerInvariant() switch
        {
            "createdat" => query.OrderByDescending(invite => invite.CreatedAt),
            "status" => query.OrderBy(invite => invite.Status),
            _ => query.OrderByDescending(invite => invite.CreatedAt)
        };

    private static IQueryable<TeamJoinRequest> ApplyJoinRequestOrdering(IQueryable<TeamJoinRequest> query, string? orderBy)
        => orderBy?.Trim().ToLowerInvariant() switch
        {
            "requestedat" => query.OrderByDescending(request => request.RequestedAt),
            "status" => query.OrderBy(request => request.Status),
            _ => query.OrderByDescending(request => request.RequestedAt)
        };

    private static TeamInviteDto MapToDto(TeamInvite invite)
        => new TeamInviteDto(
            invite.Id,
            invite.TeamId,
            invite.CreatedByUserId,
            invite.TargetUserId,
            invite.TargetEmail,
            invite.Token,
            invite.Channel,
            invite.Status,
            invite.IsPreApproved,
            invite.MaxUses,
            invite.UseCount,
            invite.Message,
            invite.CreatedAt,
            invite.ExpiresAt,
            invite.AcceptedAt);

    private static TeamJoinRequestDto MapToDto(TeamJoinRequest request)
        => new TeamJoinRequestDto(
            request.Id,
            request.TeamId,
            request.UserId,
            request.InviteId,
            request.ReviewedByUserId,
            request.Status,
            request.Source,
            request.IsAutoApproved,
            request.Message,
            request.RequestedAt,
            request.ReviewedAt);

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ETeamJoinRequestSource ResolveSource(TeamInvite invite)
        => invite.Channel switch
        {
            ETeamInviteChannel.ShareLink => ETeamJoinRequestSource.ShareLink,
            ETeamInviteChannel.QrCode => ETeamJoinRequestSource.QrCode,
            _ => ETeamJoinRequestSource.DirectInvite
        };

    private async Task<IReadOnlyDictionary<string, string>> GetTeamSettingsAsync(Guid teamId, CancellationToken cancellationToken)
    {
        var settingsId = await _dbContext.Query<TeamSettings>()
            .Where(settings => settings.TeamId == teamId)
            .Select(settings => settings.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (settingsId == Guid.Empty)
        {
            return new Dictionary<string, string>();
        }

        return await _dbContext.Query<TeamSettingEntry>()
            .Where(entry => entry.TeamSettingsId == settingsId && entry.IsEnabled)
            .ToDictionaryAsync(entry => entry.Key, entry => entry.Value, cancellationToken);
    }

    private static bool GetBoolSetting(IReadOnlyDictionary<string, string> settings, string key)
        => settings.TryGetValue(key, out var value)
           && bool.TryParse(value, out var parsed)
           && parsed;

    private static int GetIntSetting(IReadOnlyDictionary<string, string> settings, string key)
        => settings.TryGetValue(key, out var value)
           && int.TryParse(value, out var parsed)
            ? parsed
            : 0;

    private async Task<bool> CanAddMemberAsync(Guid teamId, CancellationToken cancellationToken)
    {
        var settings = await GetTeamSettingsAsync(teamId, cancellationToken);
        var maxPlayers = GetIntSetting(settings, TeamSettingKeys.MaxPlayers);
        if (maxPlayers <= 0)
        {
            return true;
        }

        var activeCount = await _dbContext.Query<TeamMember>()
            .CountAsync(member => member.TeamId == teamId && member.Status == ETeamMemberStatus.Active, cancellationToken);

        return activeCount < maxPlayers;
    }
}

using ConvocadoFc.Application.Abstractions;
using ConvocadoFc.Application.Handlers.Modules.Teams.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Teams.Models;
using ConvocadoFc.Domain.Models.Modules.Teams;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;

using Microsoft.EntityFrameworkCore;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Implementations;

public sealed class TeamPlayerHandler(IApplicationDbContext dbContext) : ITeamPlayerHandler
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<PaginatedResult<TeamPlayerDto>> ListPlayersAsync(ListTeamPlayersQuery query, CancellationToken cancellationToken)
    {
        var member = await GetMemberAsync(query.TeamId, query.CurrentUserId, cancellationToken);
        if (!query.IsSystemAdmin && (member is null || !IsModeratorOrAdmin(member)))
        {
            return new PaginatedResult<TeamPlayerDto>();
        }

        var membersQuery = _dbContext.Query<TeamMember>()
            .Where(teamMember => teamMember.TeamId == query.TeamId && teamMember.Status == ETeamMemberStatus.Active);

        membersQuery = ApplyOrdering(membersQuery, query.Pagination.OrderBy);

        var totalItems = await membersQuery.CountAsync(cancellationToken);
        var page = query.Pagination.Page <= 0 ? 1 : query.Pagination.Page;
        var pageSize = query.Pagination.PageSize <= 0 ? 20 : query.Pagination.PageSize;

        var items = await (from teamMember in membersQuery
                           join user in _dbContext.Query<ApplicationUser>() on teamMember.UserId equals user.Id
                           join profile in _dbContext.Query<TeamMemberProfile>() on teamMember.Id equals profile.TeamMemberId into profiles
                           from profile in profiles.DefaultIfEmpty()
                           select new TeamPlayerDto(
                               teamMember.Id,
                               teamMember.TeamId,
                               teamMember.UserId,
                               user.FullName,
                               teamMember.Role,
                               teamMember.Status,
                               profile != null && profile.IsFeeExempt,
                               profile != null && profile.IsOnHiatus,
                               profile != null ? profile.HiatusStartedAt : null,
                               profile != null ? profile.HiatusEndsAt : null,
                               profile != null ? profile.PrimaryPosition : null,
                               profile != null ? profile.SecondaryPosition : null,
                               profile != null ? profile.TertiaryPosition : null))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<TeamPlayerDto>
        {
            TotalItems = totalItems,
            Items = items
        };
    }

    public async Task<TeamPlayerOperationResult> UpdateMyProfileAsync(UpdateMyProfileCommand command, CancellationToken cancellationToken)
    {
        if (command.TeamId == Guid.Empty || command.UserId == Guid.Empty)
        {
            return new TeamPlayerOperationResult(ETeamPlayerOperationStatus.InvalidData, null);
        }

        var member = await _dbContext.Track<TeamMember>()
            .FirstOrDefaultAsync(existing => existing.TeamId == command.TeamId && existing.UserId == command.UserId, cancellationToken);

        if (member is null)
        {
            return new TeamPlayerOperationResult(ETeamPlayerOperationStatus.NotFound, null);
        }

        var profile = await _dbContext.Track<TeamMemberProfile>()
            .FirstOrDefaultAsync(existing => existing.TeamMemberId == member.Id, cancellationToken);

        if (profile is null)
        {
            profile = new TeamMemberProfile
            {
                Id = Guid.NewGuid(),
                TeamMemberId = member.Id,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _dbContext.AddAsync(profile, cancellationToken);
        }

        if (command.CopyFromTeamId.HasValue)
        {
            var sourceProfile = await (from sourceMember in _dbContext.Query<TeamMember>()
                                       join sourceProfileEntry in _dbContext.Query<TeamMemberProfile>() on sourceMember.Id equals sourceProfileEntry.TeamMemberId
                                       where sourceMember.TeamId == command.CopyFromTeamId.Value
                                             && sourceMember.UserId == command.UserId
                                       select sourceProfileEntry)
                .FirstOrDefaultAsync(cancellationToken);

            if (sourceProfile is not null)
            {
                profile.PrimaryPosition = sourceProfile.PrimaryPosition;
                profile.SecondaryPosition = sourceProfile.SecondaryPosition;
                profile.TertiaryPosition = sourceProfile.TertiaryPosition;
                profile.CopiedFromTeamId = command.CopyFromTeamId.Value;
            }
        }

        if (command.IsOnHiatus.HasValue || command.HiatusEndsAt.HasValue)
        {
            var settings = await GetTeamSettingsAsync(command.TeamId, cancellationToken);
            var minDays = GetIntSetting(settings, TeamSettingKeys.MinHiatusDays);
            var maxDays = GetIntSetting(settings, TeamSettingKeys.MaxHiatusDays);
            var maxPerSemester = GetIntSetting(settings, TeamSettingKeys.MaxHiatusPerSemester);
            var now = DateTimeOffset.UtcNow;

            if (command.IsOnHiatus == true)
            {
                var hiatusEndsAt = command.HiatusEndsAt;
                if (!ValidateHiatusPeriod(now, hiatusEndsAt, minDays, maxDays))
                {
                    return new TeamPlayerOperationResult(ETeamPlayerOperationStatus.InvalidData, null);
                }

                var lastStart = profile.LastHiatusStartedAt;
                var countLastSixMonths = profile.HiatusCountLast6Months;
                if (lastStart.HasValue && lastStart.Value < now.AddMonths(-6))
                {
                    countLastSixMonths = 0;
                }

                if (maxPerSemester > 0 && countLastSixMonths >= maxPerSemester)
                {
                    return new TeamPlayerOperationResult(ETeamPlayerOperationStatus.InvalidData, null);
                }

                profile.IsOnHiatus = true;
                profile.HiatusStartedAt ??= now;
                profile.HiatusEndsAt = hiatusEndsAt;
                profile.HiatusCountLast6Months = maxPerSemester > 0 ? countLastSixMonths + 1 : countLastSixMonths;
                profile.LastHiatusStartedAt = now;
            }
            else if (command.IsOnHiatus == false)
            {
                profile.IsOnHiatus = false;
                profile.HiatusStartedAt = null;
                profile.HiatusEndsAt = null;
            }
            else if (profile.IsOnHiatus)
            {
                var hiatusEndsAt = command.HiatusEndsAt ?? profile.HiatusEndsAt;
                if (!ValidateHiatusPeriod(profile.HiatusStartedAt ?? now, hiatusEndsAt, minDays, maxDays))
                {
                    return new TeamPlayerOperationResult(ETeamPlayerOperationStatus.InvalidData, null);
                }

                profile.HiatusEndsAt = hiatusEndsAt;
            }
        }

        profile.PrimaryPosition = command.PrimaryPosition;
        profile.SecondaryPosition = command.SecondaryPosition;
        profile.TertiaryPosition = command.TertiaryPosition;
        profile.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var playerDto = await MapToDtoAsync(member, profile, cancellationToken);
        return new TeamPlayerOperationResult(ETeamPlayerOperationStatus.Success, playerDto);
    }

    public async Task<TeamPlayerOperationResult> UpdatePlayerAdminAsync(UpdatePlayerAdminCommand command, CancellationToken cancellationToken)
    {
        if (command.TeamId == Guid.Empty || command.UserId == Guid.Empty || command.UpdatedByUserId == Guid.Empty)
        {
            return new TeamPlayerOperationResult(ETeamPlayerOperationStatus.InvalidData, null);
        }

        var adminMember = await GetMemberAsync(command.TeamId, command.UpdatedByUserId, cancellationToken);
        if (!command.IsSystemAdmin && (adminMember is null || adminMember.Role != ETeamMemberRole.Admin))
        {
            return new TeamPlayerOperationResult(ETeamPlayerOperationStatus.Forbidden, null);
        }

        var member = await _dbContext.Track<TeamMember>()
            .FirstOrDefaultAsync(existing => existing.TeamId == command.TeamId && existing.UserId == command.UserId, cancellationToken);

        if (member is null)
        {
            return new TeamPlayerOperationResult(ETeamPlayerOperationStatus.NotFound, null);
        }

        var profile = await _dbContext.Track<TeamMemberProfile>()
            .FirstOrDefaultAsync(existing => existing.TeamMemberId == member.Id, cancellationToken);

        if (profile is null)
        {
            profile = new TeamMemberProfile
            {
                Id = Guid.NewGuid(),
                TeamMemberId = member.Id,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _dbContext.AddAsync(profile, cancellationToken);
        }

        if (command.IsFeeExempt.HasValue)
        {
            profile.IsFeeExempt = command.IsFeeExempt.Value;
        }

        profile.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var playerDto = await MapToDtoAsync(member, profile, cancellationToken);
        return new TeamPlayerOperationResult(ETeamPlayerOperationStatus.Success, playerDto);
    }

    public async Task<TeamPlayerOperationResult> RemovePlayerAsync(RemovePlayerCommand command, CancellationToken cancellationToken)
    {
        if (command.TeamId == Guid.Empty || command.UserId == Guid.Empty || command.RemovedByUserId == Guid.Empty)
        {
            return new TeamPlayerOperationResult(ETeamPlayerOperationStatus.InvalidData, null);
        }

        var remover = await GetMemberAsync(command.TeamId, command.RemovedByUserId, cancellationToken);
        if (!command.IsSystemAdmin)
        {
            if (remover is null)
            {
                return new TeamPlayerOperationResult(ETeamPlayerOperationStatus.Forbidden, null);
            }

            if (remover.Role == ETeamMemberRole.Moderator)
            {
                var settings = await GetTeamSettingsAsync(command.TeamId, cancellationToken);
                if (!GetBoolSetting(settings, TeamSettingKeys.ModeratorsCanRemovePlayers))
                {
                    return new TeamPlayerOperationResult(ETeamPlayerOperationStatus.Forbidden, null);
                }
            }
            else if (remover.Role != ETeamMemberRole.Admin)
            {
                return new TeamPlayerOperationResult(ETeamPlayerOperationStatus.Forbidden, null);
            }
        }

        var member = await _dbContext.Track<TeamMember>()
            .FirstOrDefaultAsync(existing => existing.TeamId == command.TeamId && existing.UserId == command.UserId, cancellationToken);

        if (member is null)
        {
            return new TeamPlayerOperationResult(ETeamPlayerOperationStatus.NotFound, null);
        }

        member.Status = ETeamMemberStatus.Removed;
        member.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var playerDto = await MapToDtoAsync(member, null, cancellationToken);
        return new TeamPlayerOperationResult(ETeamPlayerOperationStatus.Success, playerDto);
    }

    private async Task<TeamMember?> GetMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken)
        => await _dbContext.Query<TeamMember>()
            .FirstOrDefaultAsync(member => member.TeamId == teamId && member.UserId == userId, cancellationToken);

    private static bool IsModeratorOrAdmin(TeamMember member)
        => member.Role == ETeamMemberRole.Admin || member.Role == ETeamMemberRole.Moderator;

    private static IQueryable<TeamMember> ApplyOrdering(IQueryable<TeamMember> query, string? orderBy)
        => orderBy?.Trim().ToLowerInvariant() switch
        {
            "joinedat" => query.OrderByDescending(member => member.JoinedAt),
            "role" => query.OrderBy(member => member.Role),
            _ => query.OrderBy(member => member.UserId)
        };

    private async Task<TeamPlayerDto> MapToDtoAsync(TeamMember member, TeamMemberProfile? profile, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Query<ApplicationUser>()
            .Where(existing => existing.Id == member.UserId)
            .Select(existing => new { existing.FullName })
            .FirstOrDefaultAsync(cancellationToken);

        return new TeamPlayerDto(
            member.Id,
            member.TeamId,
            member.UserId,
            user?.FullName ?? string.Empty,
            member.Role,
            member.Status,
            profile?.IsFeeExempt ?? false,
            profile?.IsOnHiatus ?? false,
            profile?.HiatusStartedAt,
            profile?.HiatusEndsAt,
            profile?.PrimaryPosition,
            profile?.SecondaryPosition,
            profile?.TertiaryPosition);
    }

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

    private static bool ValidateHiatusPeriod(DateTimeOffset start, DateTimeOffset? end, int minDays, int maxDays)
    {
        if (!end.HasValue)
        {
            return minDays <= 0 && maxDays <= 0;
        }

        var diffDays = (end.Value - start).TotalDays;
        if (minDays > 0 && diffDays < minDays)
        {
            return false;
        }

        if (maxDays > 0 && diffDays > maxDays)
        {
            return false;
        }

        return diffDays >= 0;
    }
}

using ConvocadoFc.Application.Abstractions;
using ConvocadoFc.Application.Handlers.Modules.Teams.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Teams.Models;
using ConvocadoFc.Domain.Models.Modules.Teams;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;

using Microsoft.EntityFrameworkCore;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Implementations;

public sealed class TeamManagementHandler(IApplicationDbContext dbContext) : ITeamManagementHandler
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<PaginatedResult<TeamDto>> ListTeamsAsync(ListTeamsQuery query, CancellationToken cancellationToken)
    {
        var teamQuery = _dbContext.Query<Team>();

        if (query.OwnerUserId.HasValue)
        {
            teamQuery = teamQuery.Where(team => team.OwnerUserId == query.OwnerUserId.Value);
        }

        teamQuery = ApplyOrdering(teamQuery, query.Pagination.OrderBy);

        var totalItems = await teamQuery.CountAsync(cancellationToken);
        var page = query.Pagination.Page <= 0 ? 1 : query.Pagination.Page;
        var pageSize = query.Pagination.PageSize <= 0 ? 20 : query.Pagination.PageSize;

        var items = await teamQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(team => MapToDto(team))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<TeamDto>
        {
            TotalItems = totalItems,
            Items = items
        };
    }

    public async Task<TeamDto?> GetTeamAsync(Guid teamId, CancellationToken cancellationToken)
        => await _dbContext.Query<Team>()
            .Where(team => team.Id == teamId)
            .Select(team => MapToDto(team))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<TeamOperationResult> CreateTeamAsync(CreateTeamCommand command, CancellationToken cancellationToken)
    {
        if (!command.IsSystemAdmin)
        {
            return new TeamOperationResult(ETeamOperationStatus.Forbidden, null);
        }

        if (string.IsNullOrWhiteSpace(command.Name) || string.IsNullOrWhiteSpace(command.HomeFieldName))
        {
            return new TeamOperationResult(ETeamOperationStatus.InvalidData, null);
        }

        var ownerExists = await _dbContext.Query<ApplicationUser>()
            .AnyAsync(user => user.Id == command.OwnerUserId, cancellationToken);

        if (!ownerExists)
        {
            return new TeamOperationResult(ETeamOperationStatus.UserNotFound, null);
        }

        var normalizedName = NormalizeName(command.Name);
        var normalizedFieldName = NormalizeName(command.HomeFieldName);

        var nameExists = await _dbContext.Query<Team>()
            .AnyAsync(team => team.OwnerUserId == command.OwnerUserId && team.Name == normalizedName, cancellationToken);

        if (nameExists)
        {
            return new TeamOperationResult(ETeamOperationStatus.NameAlreadyExists, null);
        }

        var team = new Team
        {
            Id = Guid.NewGuid(),
            OwnerUserId = command.OwnerUserId,
            Name = normalizedName,
            HomeFieldName = normalizedFieldName,
            HomeFieldAddress = NormalizeNullable(command.HomeFieldAddress),
            HomeFieldLatitude = command.HomeFieldLatitude,
            HomeFieldLongitude = command.HomeFieldLongitude,
            CrestUrl = NormalizeNullable(command.CrestUrl),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var settings = new TeamSettings
        {
            Id = Guid.NewGuid(),
            TeamId = team.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var ownerMember = new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = team.Id,
            UserId = command.OwnerUserId,
            AddedByUserId = command.OwnerUserId,
            Role = ETeamMemberRole.Admin,
            Status = ETeamMemberStatus.Active,
            JoinedAt = DateTimeOffset.UtcNow
        };

        await _dbContext.ExecuteInTransactionAsync(async ct =>
        {
            await _dbContext.AddAsync(team, ct);
            await _dbContext.AddAsync(settings, ct);
            await _dbContext.AddAsync(ownerMember, ct);
            await _dbContext.SaveChangesAsync(ct);
        }, cancellationToken);

        return new TeamOperationResult(ETeamOperationStatus.Success, MapToDto(team));
    }

    public async Task<TeamOperationResult> UpdateTeamAsync(UpdateTeamCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name) || string.IsNullOrWhiteSpace(command.HomeFieldName))
        {
            return new TeamOperationResult(ETeamOperationStatus.InvalidData, null);
        }

        var team = await _dbContext.Track<Team>()
            .FirstOrDefaultAsync(existing => existing.Id == command.TeamId, cancellationToken);

        if (team is null)
        {
            return new TeamOperationResult(ETeamOperationStatus.NotFound, null);
        }

        if (!command.IsSystemAdmin && team.OwnerUserId != command.UpdatedByUserId)
        {
            var isTeamAdmin = await _dbContext.Query<TeamMember>()
                .AnyAsync(member => member.TeamId == team.Id
                                     && member.UserId == command.UpdatedByUserId
                                     && member.Role == ETeamMemberRole.Admin
                                     && member.Status == ETeamMemberStatus.Active, cancellationToken);

            if (!isTeamAdmin)
            {
                return new TeamOperationResult(ETeamOperationStatus.Forbidden, null);
            }
        }

        var normalizedName = NormalizeName(command.Name);
        var normalizedFieldName = NormalizeName(command.HomeFieldName);

        var nameExists = await _dbContext.Query<Team>()
            .AnyAsync(existing => existing.OwnerUserId == team.OwnerUserId
                                  && existing.Name == normalizedName
                                  && existing.Id != team.Id, cancellationToken);

        if (nameExists)
        {
            return new TeamOperationResult(ETeamOperationStatus.NameAlreadyExists, null);
        }

        team.Name = normalizedName;
        team.HomeFieldName = normalizedFieldName;
        team.HomeFieldAddress = NormalizeNullable(command.HomeFieldAddress);
        team.HomeFieldLatitude = command.HomeFieldLatitude;
        team.HomeFieldLongitude = command.HomeFieldLongitude;
        team.CrestUrl = NormalizeNullable(command.CrestUrl);
        team.IsActive = command.IsActive;
        team.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TeamOperationResult(ETeamOperationStatus.Success, MapToDto(team));
    }

    public async Task<TeamOperationResult> RemoveTeamAsync(Guid teamId, Guid removedByUserId, bool isSystemAdmin, CancellationToken cancellationToken)
    {
        var team = await _dbContext.Track<Team>()
            .FirstOrDefaultAsync(existing => existing.Id == teamId, cancellationToken);

        if (team is null)
        {
            return new TeamOperationResult(ETeamOperationStatus.NotFound, null);
        }

        if (!isSystemAdmin && team.OwnerUserId != removedByUserId)
        {
            var isTeamAdmin = await _dbContext.Query<TeamMember>()
                .AnyAsync(member => member.TeamId == team.Id
                                     && member.UserId == removedByUserId
                                     && member.Role == ETeamMemberRole.Admin
                                     && member.Status == ETeamMemberStatus.Active, cancellationToken);

            if (!isTeamAdmin)
            {
                return new TeamOperationResult(ETeamOperationStatus.Forbidden, null);
            }
        }

        team.IsActive = false;
        team.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TeamOperationResult(ETeamOperationStatus.Success, MapToDto(team));
    }

    private static IQueryable<Team> ApplyOrdering(IQueryable<Team> query, string? orderBy)
        => orderBy?.Trim().ToLowerInvariant() switch
        {
            "createdat" => query.OrderByDescending(team => team.CreatedAt),
            "updatedat" => query.OrderByDescending(team => team.UpdatedAt ?? team.CreatedAt),
            _ => query.OrderBy(team => team.Name)
        };

    private static TeamDto MapToDto(Team team)
        => new TeamDto(
            team.Id,
            team.OwnerUserId,
            team.Name,
            team.HomeFieldName,
            team.HomeFieldAddress,
            team.HomeFieldLatitude,
            team.HomeFieldLongitude,
            team.CrestUrl,
            team.IsActive,
            team.CreatedAt,
            team.UpdatedAt);

    private static string NormalizeName(string value)
        => value.Trim();

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

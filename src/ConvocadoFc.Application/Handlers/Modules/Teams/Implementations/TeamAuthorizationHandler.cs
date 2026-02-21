using ConvocadoFc.Application.Abstractions;
using ConvocadoFc.Application.Handlers.Modules.Teams.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Teams.Models;
using ConvocadoFc.Domain.Models.Modules.Teams;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;

using Microsoft.EntityFrameworkCore;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Implementations;

public sealed class TeamAuthorizationHandler(IApplicationDbContext dbContext) : ITeamAuthorizationHandler
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<IReadOnlyCollection<TeamModeratorDto>> ListModeratorsAsync(Guid teamId, Guid currentUserId, bool isSystemAdmin, CancellationToken cancellationToken)
    {
        var member = await GetMemberAsync(teamId, currentUserId, cancellationToken);
        if (!isSystemAdmin && (member is null || member.Role != ETeamMemberRole.Admin))
        {
            return Array.Empty<TeamModeratorDto>();
        }

        var moderators = await (from teamMember in _dbContext.Query<TeamMember>()
                                join user in _dbContext.Query<ApplicationUser>() on teamMember.UserId equals user.Id
                                where teamMember.TeamId == teamId
                                      && teamMember.Role == ETeamMemberRole.Moderator
                                      && teamMember.Status == ETeamMemberStatus.Active
                                select new TeamModeratorDto(
                                    teamMember.TeamId,
                                    teamMember.UserId,
                                    user.FullName,
                                    teamMember.Role))
            .ToListAsync(cancellationToken);

        return moderators;
    }

    public async Task<TeamAuthorizationOperationResult> AssignModeratorAsync(AssignModeratorCommand command, CancellationToken cancellationToken)
    {
        if (command.TeamId == Guid.Empty || command.UserId == Guid.Empty || command.AssignedByUserId == Guid.Empty)
        {
            return new TeamAuthorizationOperationResult(ETeamAuthorizationOperationStatus.InvalidData, null);
        }

        var adminMember = await GetMemberAsync(command.TeamId, command.AssignedByUserId, cancellationToken);
        if (!command.IsSystemAdmin && (adminMember is null || adminMember.Role != ETeamMemberRole.Admin))
        {
            return new TeamAuthorizationOperationResult(ETeamAuthorizationOperationStatus.Forbidden, null);
        }

        var member = await _dbContext.Track<TeamMember>()
            .FirstOrDefaultAsync(existing => existing.TeamId == command.TeamId && existing.UserId == command.UserId, cancellationToken);

        if (member is null)
        {
            return new TeamAuthorizationOperationResult(ETeamAuthorizationOperationStatus.NotFound, null);
        }

        member.Role = ETeamMemberRole.Moderator;
        member.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var userName = await _dbContext.Query<ApplicationUser>()
            .Where(user => user.Id == member.UserId)
            .Select(user => user.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new TeamModeratorDto(member.TeamId, member.UserId, userName ?? string.Empty, member.Role);
        return new TeamAuthorizationOperationResult(ETeamAuthorizationOperationStatus.Success, dto);
    }

    public async Task<TeamAuthorizationOperationResult> RemoveModeratorAsync(RemoveModeratorCommand command, CancellationToken cancellationToken)
    {
        if (command.TeamId == Guid.Empty || command.UserId == Guid.Empty || command.RemovedByUserId == Guid.Empty)
        {
            return new TeamAuthorizationOperationResult(ETeamAuthorizationOperationStatus.InvalidData, null);
        }

        var adminMember = await GetMemberAsync(command.TeamId, command.RemovedByUserId, cancellationToken);
        if (!command.IsSystemAdmin && (adminMember is null || adminMember.Role != ETeamMemberRole.Admin))
        {
            return new TeamAuthorizationOperationResult(ETeamAuthorizationOperationStatus.Forbidden, null);
        }

        var member = await _dbContext.Track<TeamMember>()
            .FirstOrDefaultAsync(existing => existing.TeamId == command.TeamId && existing.UserId == command.UserId, cancellationToken);

        if (member is null)
        {
            return new TeamAuthorizationOperationResult(ETeamAuthorizationOperationStatus.NotFound, null);
        }

        member.Role = ETeamMemberRole.User;
        member.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var userName = await _dbContext.Query<ApplicationUser>()
            .Where(user => user.Id == member.UserId)
            .Select(user => user.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        var dto = new TeamModeratorDto(member.TeamId, member.UserId, userName ?? string.Empty, member.Role);
        return new TeamAuthorizationOperationResult(ETeamAuthorizationOperationStatus.Success, dto);
    }

    private async Task<TeamMember?> GetMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken)
        => await _dbContext.Query<TeamMember>()
            .FirstOrDefaultAsync(member => member.TeamId == teamId && member.UserId == userId, cancellationToken);
}

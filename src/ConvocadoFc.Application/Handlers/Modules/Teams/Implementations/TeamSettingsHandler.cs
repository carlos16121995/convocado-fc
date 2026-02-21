using ConvocadoFc.Application.Abstractions;
using ConvocadoFc.Application.Handlers.Modules.Teams.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Teams.Models;
using ConvocadoFc.Domain.Models.Modules.Teams;

using Microsoft.EntityFrameworkCore;

namespace ConvocadoFc.Application.Handlers.Modules.Teams.Implementations;

public sealed class TeamSettingsHandler(IApplicationDbContext dbContext) : ITeamSettingsHandler
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<TeamSettingsDto?> GetSettingsAsync(Guid teamId, Guid currentUserId, bool isSystemAdmin, CancellationToken cancellationToken)
    {
        var member = await GetMemberAsync(teamId, currentUserId, cancellationToken);
        if (!isSystemAdmin && (member is null || member.Role != ETeamMemberRole.Admin))
        {
            return null;
        }

        var settings = await EnsureSettingsAsync(teamId, cancellationToken);
        return await MapToDtoAsync(settings.TeamId, cancellationToken);
    }

    public async Task<TeamSettingsOperationResult> UpsertSettingsAsync(UpsertTeamSettingsCommand command, CancellationToken cancellationToken)
    {
        if (command.TeamId == Guid.Empty || command.UpdatedByUserId == Guid.Empty)
        {
            return new TeamSettingsOperationResult(ETeamSettingsOperationStatus.InvalidData, null);
        }

        var member = await GetMemberAsync(command.TeamId, command.UpdatedByUserId, cancellationToken);
        if (!command.IsSystemAdmin && (member is null || member.Role != ETeamMemberRole.Admin))
        {
            return new TeamSettingsOperationResult(ETeamSettingsOperationStatus.Forbidden, null);
        }

        var settings = await EnsureSettingsAsync(command.TeamId, cancellationToken);

        foreach (var entry in command.Settings)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
            {
                return new TeamSettingsOperationResult(ETeamSettingsOperationStatus.InvalidData, null);
            }
        }

        var existing = await _dbContext.Track<TeamSettingEntry>()
            .Where(item => item.TeamSettingsId == settings.Id)
            .ToListAsync(cancellationToken);

        foreach (var incoming in command.Settings)
        {
            var match = existing.FirstOrDefault(item => item.Key == incoming.Key);
            if (match is null)
            {
                match = new TeamSettingEntry
                {
                    Id = Guid.NewGuid(),
                    TeamSettingsId = settings.Id,
                    Key = incoming.Key.Trim(),
                    Value = incoming.Value.Trim(),
                    ValueType = NormalizeNullable(incoming.ValueType),
                    IsEnabled = incoming.IsEnabled,
                    Description = NormalizeNullable(incoming.Description)
                };

                await _dbContext.AddAsync(match, cancellationToken);
                existing.Add(match);
            }
            else
            {
                match.Value = incoming.Value.Trim();
                match.ValueType = NormalizeNullable(incoming.ValueType);
                match.IsEnabled = incoming.IsEnabled;
                match.Description = NormalizeNullable(incoming.Description);
            }
        }

        settings.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = await MapToDtoAsync(settings.TeamId, cancellationToken);
        return new TeamSettingsOperationResult(ETeamSettingsOperationStatus.Success, dto);
    }

    public async Task<TeamRuleOperationResult> CreateRuleAsync(CreateTeamRuleCommand command, CancellationToken cancellationToken)
    {
        if (command.TeamId == Guid.Empty || command.CreatedByUserId == Guid.Empty)
        {
            return new TeamRuleOperationResult(ETeamRuleOperationStatus.InvalidData, null);
        }

        if (string.IsNullOrWhiteSpace(command.Code) || string.IsNullOrWhiteSpace(command.Name))
        {
            return new TeamRuleOperationResult(ETeamRuleOperationStatus.InvalidData, null);
        }

        var member = await GetMemberAsync(command.TeamId, command.CreatedByUserId, cancellationToken);
        if (!command.IsSystemAdmin && (member is null || member.Role != ETeamMemberRole.Admin))
        {
            return new TeamRuleOperationResult(ETeamRuleOperationStatus.Forbidden, null);
        }

        var settings = await EnsureSettingsAsync(command.TeamId, cancellationToken);

        var rule = new TeamRule
        {
            Id = Guid.NewGuid(),
            TeamSettingsId = settings.Id,
            Code = command.Code.Trim(),
            Name = command.Name.Trim(),
            Description = NormalizeNullable(command.Description),
            Scope = NormalizeNullable(command.Scope),
            Target = NormalizeNullable(command.Target),
            IsEnabled = command.IsEnabled,
            StartsAt = command.StartsAt,
            EndsAt = command.EndsAt
        };

        await _dbContext.AddAsync(rule, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TeamRuleOperationResult(ETeamRuleOperationStatus.Success, await MapToDtoAsync(rule, cancellationToken));
    }

    public async Task<TeamRuleOperationResult> UpdateRuleAsync(UpdateTeamRuleCommand command, CancellationToken cancellationToken)
    {
        if (command.RuleId == Guid.Empty || command.UpdatedByUserId == Guid.Empty)
        {
            return new TeamRuleOperationResult(ETeamRuleOperationStatus.InvalidData, null);
        }

        var rule = await _dbContext.Track<TeamRule>()
            .FirstOrDefaultAsync(existing => existing.Id == command.RuleId, cancellationToken);

        if (rule is null)
        {
            return new TeamRuleOperationResult(ETeamRuleOperationStatus.NotFound, null);
        }

        var member = await GetMemberAsyncBySettingsId(rule.TeamSettingsId, command.UpdatedByUserId, cancellationToken);
        if (!command.IsSystemAdmin && (member is null || member.Role != ETeamMemberRole.Admin))
        {
            return new TeamRuleOperationResult(ETeamRuleOperationStatus.Forbidden, null);
        }

        rule.Name = command.Name.Trim();
        rule.Description = NormalizeNullable(command.Description);
        rule.Scope = NormalizeNullable(command.Scope);
        rule.Target = NormalizeNullable(command.Target);
        rule.IsEnabled = command.IsEnabled;
        rule.StartsAt = command.StartsAt;
        rule.EndsAt = command.EndsAt;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TeamRuleOperationResult(ETeamRuleOperationStatus.Success, await MapToDtoAsync(rule, cancellationToken));
    }

    public async Task<TeamRuleOperationResult> RemoveRuleAsync(RemoveTeamRuleCommand command, CancellationToken cancellationToken)
    {
        if (command.RuleId == Guid.Empty || command.RemovedByUserId == Guid.Empty)
        {
            return new TeamRuleOperationResult(ETeamRuleOperationStatus.InvalidData, null);
        }

        var rule = await _dbContext.Track<TeamRule>()
            .FirstOrDefaultAsync(existing => existing.Id == command.RuleId, cancellationToken);

        if (rule is null)
        {
            return new TeamRuleOperationResult(ETeamRuleOperationStatus.NotFound, null);
        }

        var member = await GetMemberAsyncBySettingsId(rule.TeamSettingsId, command.RemovedByUserId, cancellationToken);
        if (!command.IsSystemAdmin && (member is null || member.Role != ETeamMemberRole.Admin))
        {
            return new TeamRuleOperationResult(ETeamRuleOperationStatus.Forbidden, null);
        }

        await _dbContext.RemoveAsync(rule, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TeamRuleOperationResult(ETeamRuleOperationStatus.Success, null);
    }

    public async Task<TeamRuleParameterOperationResult> AddRuleParameterAsync(AddTeamRuleParameterCommand command, CancellationToken cancellationToken)
    {
        if (command.RuleId == Guid.Empty || command.AddedByUserId == Guid.Empty)
        {
            return new TeamRuleParameterOperationResult(ETeamRuleParameterOperationStatus.InvalidData, null);
        }

        if (string.IsNullOrWhiteSpace(command.Key))
        {
            return new TeamRuleParameterOperationResult(ETeamRuleParameterOperationStatus.InvalidData, null);
        }

        var rule = await _dbContext.Track<TeamRule>()
            .FirstOrDefaultAsync(existing => existing.Id == command.RuleId, cancellationToken);

        if (rule is null)
        {
            return new TeamRuleParameterOperationResult(ETeamRuleParameterOperationStatus.NotFound, null);
        }

        var member = await GetMemberAsyncBySettingsId(rule.TeamSettingsId, command.AddedByUserId, cancellationToken);
        if (!command.IsSystemAdmin && (member is null || member.Role != ETeamMemberRole.Admin))
        {
            return new TeamRuleParameterOperationResult(ETeamRuleParameterOperationStatus.Forbidden, null);
        }

        var parameter = new TeamRuleParameter
        {
            Id = Guid.NewGuid(),
            TeamRuleId = rule.Id,
            Key = command.Key.Trim(),
            Value = command.Value.Trim(),
            ValueType = NormalizeNullable(command.ValueType),
            Unit = NormalizeNullable(command.Unit),
            Description = NormalizeNullable(command.Description)
        };

        await _dbContext.AddAsync(parameter, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TeamRuleParameterOperationResult(ETeamRuleParameterOperationStatus.Success, MapToDto(parameter));
    }

    public async Task<TeamRuleParameterOperationResult> RemoveRuleParameterAsync(RemoveTeamRuleParameterCommand command, CancellationToken cancellationToken)
    {
        if (command.RuleParameterId == Guid.Empty || command.RemovedByUserId == Guid.Empty)
        {
            return new TeamRuleParameterOperationResult(ETeamRuleParameterOperationStatus.InvalidData, null);
        }

        var parameter = await _dbContext.Track<TeamRuleParameter>()
            .FirstOrDefaultAsync(existing => existing.Id == command.RuleParameterId, cancellationToken);

        if (parameter is null)
        {
            return new TeamRuleParameterOperationResult(ETeamRuleParameterOperationStatus.NotFound, null);
        }

        var rule = await _dbContext.Query<TeamRule>()
            .FirstOrDefaultAsync(existing => existing.Id == parameter.TeamRuleId, cancellationToken);

        if (rule is null)
        {
            return new TeamRuleParameterOperationResult(ETeamRuleParameterOperationStatus.NotFound, null);
        }

        var member = await GetMemberAsyncBySettingsId(rule.TeamSettingsId, command.RemovedByUserId, cancellationToken);
        if (!command.IsSystemAdmin && (member is null || member.Role != ETeamMemberRole.Admin))
        {
            return new TeamRuleParameterOperationResult(ETeamRuleParameterOperationStatus.Forbidden, null);
        }

        await _dbContext.RemoveAsync(parameter, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TeamRuleParameterOperationResult(ETeamRuleParameterOperationStatus.Success, null);
    }

    private async Task<TeamSettings> EnsureSettingsAsync(Guid teamId, CancellationToken cancellationToken)
    {
        var settings = await _dbContext.Track<TeamSettings>()
            .FirstOrDefaultAsync(existing => existing.TeamId == teamId, cancellationToken);

        if (settings is not null)
        {
            return settings;
        }

        var newSettings = new TeamSettings
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _dbContext.AddAsync(newSettings, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return newSettings;
    }

    private async Task<TeamMember?> GetMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken)
        => await _dbContext.Query<TeamMember>()
            .FirstOrDefaultAsync(member => member.TeamId == teamId && member.UserId == userId, cancellationToken);

    private async Task<TeamMember?> GetMemberAsyncBySettingsId(Guid teamSettingsId, Guid userId, CancellationToken cancellationToken)
    {
        var teamId = await _dbContext.Query<TeamSettings>()
            .Where(settings => settings.Id == teamSettingsId)
            .Select(settings => settings.TeamId)
            .FirstOrDefaultAsync(cancellationToken);

        if (teamId == Guid.Empty)
        {
            return null;
        }

        return await GetMemberAsync(teamId, userId, cancellationToken);
    }

    private async Task<TeamSettingsDto> MapToDtoAsync(Guid teamId, CancellationToken cancellationToken)
    {
        var settings = await _dbContext.Query<TeamSettings>()
            .Where(item => item.TeamId == teamId)
            .Select(item => item.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var entries = await _dbContext.Query<TeamSettingEntry>()
            .Where(entry => entry.TeamSettingsId == settings)
            .Select(entry => new TeamSettingEntryDto(
                entry.Id,
                entry.Key,
                entry.Value,
                entry.ValueType,
                entry.IsEnabled,
                entry.Description))
            .ToListAsync(cancellationToken);

        var rules = await _dbContext.Query<TeamRule>()
            .Where(rule => rule.TeamSettingsId == settings)
            .Select(rule => new TeamRuleDto(
                rule.Id,
                rule.Code,
                rule.Name,
                rule.Description,
                rule.Scope,
                rule.Target,
                rule.IsEnabled,
                rule.StartsAt,
                rule.EndsAt,
                _dbContext.Query<TeamRuleParameter>()
                    .Where(parameter => parameter.TeamRuleId == rule.Id)
                    .Select(parameter => new TeamRuleParameterDto(
                        parameter.Id,
                        parameter.Key,
                        parameter.Value,
                        parameter.ValueType,
                        parameter.Unit,
                        parameter.Description))
                    .ToList()))
            .ToListAsync(cancellationToken);

        return new TeamSettingsDto(teamId, entries, rules);
    }

    private static TeamRuleDto MapToDto(TeamRule rule, IQueryable<TeamRuleParameter> parameters)
        => new TeamRuleDto(
            rule.Id,
            rule.Code,
            rule.Name,
            rule.Description,
            rule.Scope,
            rule.Target,
            rule.IsEnabled,
            rule.StartsAt,
            rule.EndsAt,
            parameters.Select(MapToDto).ToList());

    private async Task<TeamRuleDto> MapToDtoAsync(TeamRule rule, CancellationToken cancellationToken)
    {
        var parameters = await _dbContext.Query<TeamRuleParameter>()
            .Where(parameter => parameter.TeamRuleId == rule.Id)
            .Select(parameter => new TeamRuleParameterDto(
                parameter.Id,
                parameter.Key,
                parameter.Value,
                parameter.ValueType,
                parameter.Unit,
                parameter.Description))
            .ToListAsync(cancellationToken);

        return new TeamRuleDto(
            rule.Id,
            rule.Code,
            rule.Name,
            rule.Description,
            rule.Scope,
            rule.Target,
            rule.IsEnabled,
            rule.StartsAt,
            rule.EndsAt,
            parameters);
    }

    private static TeamRuleParameterDto MapToDto(TeamRuleParameter parameter)
        => new TeamRuleParameterDto(
            parameter.Id,
            parameter.Key,
            parameter.Value,
            parameter.ValueType,
            parameter.Unit,
            parameter.Description);

    private static string? NormalizeNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

using ConvocadoFc.Application.Handlers.Modules.Teams.Implementations;
using ConvocadoFc.Application.Handlers.Modules.Teams.Models;
using ConvocadoFc.Domain.Models.Modules.Teams;
using ConvocadoFc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ConvocadoFc.Application.Tests;

public sealed class TeamSettingsHandlerTests
{
    [Fact]
    public async Task GetSettingsAsync_WhenNotAdmin_ReturnsNull()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = userId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamSettingsHandler(context);

        var result = await handler.GetSettingsAsync(teamId, userId, false, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetSettingsAsync_WhenAdmin_ReturnsSettings()
    {
        var teamId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = adminId, Role = ETeamMemberRole.Admin, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamSettingsHandler(context);

        var result = await handler.GetSettingsAsync(teamId, adminId, false, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(teamId, result!.TeamId);
    }

    [Fact]
    public async Task UpsertSettingsAsync_WhenInvalidData_ReturnsInvalidData()
    {
        await using var context = CreateContext();
        var handler = new TeamSettingsHandler(context);

        var result = await handler.UpsertSettingsAsync(new UpsertTeamSettingsCommand(
            Guid.Empty,
            Guid.Empty,
            Array.Empty<UpsertTeamSettingEntry>(),
            false),
            CancellationToken.None);

        Assert.Equal(ETeamSettingsOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task UpsertSettingsAsync_WhenForbidden_ReturnsForbidden()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = userId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamSettingsHandler(context);

        var result = await handler.UpsertSettingsAsync(new UpsertTeamSettingsCommand(
            teamId,
            userId,
            new[] { new UpsertTeamSettingEntry("Key", "Value", null, true, null) },
            false),
            CancellationToken.None);

        Assert.Equal(ETeamSettingsOperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task UpsertSettingsAsync_WhenEntryKeyInvalid_ReturnsInvalidData()
    {
        var teamId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = adminId, Role = ETeamMemberRole.Admin, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamSettingsHandler(context);

        var result = await handler.UpsertSettingsAsync(new UpsertTeamSettingsCommand(
            teamId,
            adminId,
            new[] { new UpsertTeamSettingEntry(" ", "Value", null, true, null) },
            false),
            CancellationToken.None);

        Assert.Equal(ETeamSettingsOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task UpsertSettingsAsync_WhenSuccess_UpsertsEntries()
    {
        var teamId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = adminId, Role = ETeamMemberRole.Admin, Status = ETeamMemberStatus.Active });
        context.TeamSettings.Add(new TeamSettings { TeamId = teamId });
        await context.SaveChangesAsync();

        var handler = new TeamSettingsHandler(context);

        var result = await handler.UpsertSettingsAsync(new UpsertTeamSettingsCommand(
            teamId,
            adminId,
            new[]
            {
                new UpsertTeamSettingEntry(" Key1 ", " Value1 ", "string", true, "desc"),
                new UpsertTeamSettingEntry("Key2", "Value2", null, false, null)
            },
            false),
            CancellationToken.None);

        Assert.Equal(ETeamSettingsOperationStatus.Success, result.Status);
        Assert.Equal(2, result.Settings!.Settings.Count);
    }

    [Fact]
    public async Task CreateRuleAsync_WhenInvalidData_ReturnsInvalidData()
    {
        await using var context = CreateContext();
        var handler = new TeamSettingsHandler(context);

        var result = await handler.CreateRuleAsync(new CreateTeamRuleCommand(
            Guid.Empty,
            Guid.Empty,
            "",
            "",
            null,
            null,
            null,
            true,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task CreateRuleAsync_WhenForbidden_ReturnsForbidden()
    {
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = userId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamSettingsHandler(context);

        var result = await handler.CreateRuleAsync(new CreateTeamRuleCommand(
            teamId,
            userId,
            "CODE",
            "Rule",
            null,
            null,
            null,
            true,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleOperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task CreateRuleAsync_WhenSuccess_ReturnsRule()
    {
        var teamId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = adminId, Role = ETeamMemberRole.Admin, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamSettingsHandler(context);

        var result = await handler.CreateRuleAsync(new CreateTeamRuleCommand(
            teamId,
            adminId,
            " CODE ",
            " Name ",
            " desc ",
            " scope ",
            " target ",
            true,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleOperationStatus.Success, result.Status);
        Assert.NotNull(result.Rule);
        Assert.Equal("CODE", result.Rule!.Code);
    }

    [Fact]
    public async Task UpdateRuleAsync_WhenInvalidData_ReturnsInvalidData()
    {
        await using var context = CreateContext();
        var handler = new TeamSettingsHandler(context);

        var result = await handler.UpdateRuleAsync(new UpdateTeamRuleCommand(
            Guid.Empty,
            Guid.Empty,
            "Name",
            null,
            null,
            null,
            true,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task UpdateRuleAsync_WhenNotFound_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var handler = new TeamSettingsHandler(context);

        var result = await handler.UpdateRuleAsync(new UpdateTeamRuleCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Name",
            null,
            null,
            null,
            true,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task UpdateRuleAsync_WhenForbidden_ReturnsForbidden()
    {
        var teamId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        var settings = new TeamSettings { TeamId = teamId };
        context.TeamSettings.Add(settings);
        context.TeamRules.Add(new TeamRule { Id = ruleId, TeamSettingsId = settings.Id, Code = "CODE", Name = "Rule" });
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = userId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamSettingsHandler(context);

        var result = await handler.UpdateRuleAsync(new UpdateTeamRuleCommand(
            ruleId,
            userId,
            "Name",
            null,
            null,
            null,
            true,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleOperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task UpdateRuleAsync_WhenSuccess_UpdatesRule()
    {
        var teamId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        var settings = new TeamSettings { TeamId = teamId };
        context.TeamSettings.Add(settings);
        context.TeamRules.Add(new TeamRule { Id = ruleId, TeamSettingsId = settings.Id, Code = "CODE", Name = "Rule" });
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = adminId, Role = ETeamMemberRole.Admin, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamSettingsHandler(context);

        var result = await handler.UpdateRuleAsync(new UpdateTeamRuleCommand(
            ruleId,
            adminId,
            " Name2 ",
            " desc ",
            " scope ",
            " target ",
            false,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleOperationStatus.Success, result.Status);
        Assert.Equal("Name2", result.Rule!.Name);
    }

    [Fact]
    public async Task RemoveRuleAsync_WhenInvalidData_ReturnsInvalidData()
    {
        await using var context = CreateContext();
        var handler = new TeamSettingsHandler(context);

        var result = await handler.RemoveRuleAsync(new RemoveTeamRuleCommand(
            Guid.Empty,
            Guid.Empty,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task RemoveRuleAsync_WhenNotFound_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var handler = new TeamSettingsHandler(context);

        var result = await handler.RemoveRuleAsync(new RemoveTeamRuleCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task RemoveRuleAsync_WhenForbidden_ReturnsForbidden()
    {
        var teamId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        var settings = new TeamSettings { TeamId = teamId };
        context.TeamSettings.Add(settings);
        context.TeamRules.Add(new TeamRule { Id = ruleId, TeamSettingsId = settings.Id, Code = "CODE", Name = "Rule" });
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = userId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamSettingsHandler(context);

        var result = await handler.RemoveRuleAsync(new RemoveTeamRuleCommand(
            ruleId,
            userId,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleOperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task RemoveRuleAsync_WhenSuccess_RemovesRule()
    {
        var teamId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        var settings = new TeamSettings { TeamId = teamId };
        context.TeamSettings.Add(settings);
        context.TeamRules.Add(new TeamRule { Id = ruleId, TeamSettingsId = settings.Id, Code = "CODE", Name = "Rule" });
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = adminId, Role = ETeamMemberRole.Admin, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamSettingsHandler(context);

        var result = await handler.RemoveRuleAsync(new RemoveTeamRuleCommand(
            ruleId,
            adminId,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleOperationStatus.Success, result.Status);
        Assert.Empty(context.TeamRules);
    }

    [Fact]
    public async Task AddRuleParameterAsync_WhenInvalidData_ReturnsInvalidData()
    {
        await using var context = CreateContext();
        var handler = new TeamSettingsHandler(context);

        var result = await handler.AddRuleParameterAsync(new AddTeamRuleParameterCommand(
            Guid.Empty,
            Guid.Empty,
            "",
            "",
            null,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleParameterOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task AddRuleParameterAsync_WhenRuleNotFound_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var handler = new TeamSettingsHandler(context);

        var result = await handler.AddRuleParameterAsync(new AddTeamRuleParameterCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Key",
            "Value",
            null,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleParameterOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task AddRuleParameterAsync_WhenForbidden_ReturnsForbidden()
    {
        var teamId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        var settings = new TeamSettings { TeamId = teamId };
        context.TeamSettings.Add(settings);
        context.TeamRules.Add(new TeamRule { Id = ruleId, TeamSettingsId = settings.Id, Code = "CODE", Name = "Rule" });
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = userId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamSettingsHandler(context);

        var result = await handler.AddRuleParameterAsync(new AddTeamRuleParameterCommand(
            ruleId,
            userId,
            "Key",
            "Value",
            null,
            null,
            null,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleParameterOperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task AddRuleParameterAsync_WhenSuccess_ReturnsParameter()
    {
        var teamId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        var settings = new TeamSettings { TeamId = teamId };
        context.TeamSettings.Add(settings);
        context.TeamRules.Add(new TeamRule { Id = ruleId, TeamSettingsId = settings.Id, Code = "CODE", Name = "Rule" });
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = adminId, Role = ETeamMemberRole.Admin, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamSettingsHandler(context);

        var result = await handler.AddRuleParameterAsync(new AddTeamRuleParameterCommand(
            ruleId,
            adminId,
            " Key ",
            " Value ",
            "string",
            "unit",
            "desc",
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleParameterOperationStatus.Success, result.Status);
        Assert.Equal("Key", result.Parameter!.Key);
    }

    [Fact]
    public async Task RemoveRuleParameterAsync_WhenInvalidData_ReturnsInvalidData()
    {
        await using var context = CreateContext();
        var handler = new TeamSettingsHandler(context);

        var result = await handler.RemoveRuleParameterAsync(new RemoveTeamRuleParameterCommand(
            Guid.Empty,
            Guid.Empty,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleParameterOperationStatus.InvalidData, result.Status);
    }

    [Fact]
    public async Task RemoveRuleParameterAsync_WhenParameterNotFound_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var handler = new TeamSettingsHandler(context);

        var result = await handler.RemoveRuleParameterAsync(new RemoveTeamRuleParameterCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleParameterOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task RemoveRuleParameterAsync_WhenRuleNotFound_ReturnsNotFound()
    {
        var parameterId = Guid.NewGuid();

        await using var context = CreateContext();
        context.TeamRuleParameters.Add(new TeamRuleParameter { Id = parameterId, TeamRuleId = Guid.NewGuid(), Key = "Key", Value = "Value" });
        await context.SaveChangesAsync();

        var handler = new TeamSettingsHandler(context);

        var result = await handler.RemoveRuleParameterAsync(new RemoveTeamRuleParameterCommand(
            parameterId,
            Guid.NewGuid(),
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleParameterOperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task RemoveRuleParameterAsync_WhenForbidden_ReturnsForbidden()
    {
        var teamId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var parameterId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        var settings = new TeamSettings { TeamId = teamId };
        context.TeamSettings.Add(settings);
        context.TeamRules.Add(new TeamRule { Id = ruleId, TeamSettingsId = settings.Id, Code = "CODE", Name = "Rule" });
        context.TeamRuleParameters.Add(new TeamRuleParameter { Id = parameterId, TeamRuleId = ruleId, Key = "Key", Value = "Value" });
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = userId, Role = ETeamMemberRole.User, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamSettingsHandler(context);

        var result = await handler.RemoveRuleParameterAsync(new RemoveTeamRuleParameterCommand(
            parameterId,
            userId,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleParameterOperationStatus.Forbidden, result.Status);
    }

    [Fact]
    public async Task RemoveRuleParameterAsync_WhenSuccess_RemovesParameter()
    {
        var teamId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var parameterId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Teams.Add(CreateTeam(teamId));
        var settings = new TeamSettings { TeamId = teamId };
        context.TeamSettings.Add(settings);
        context.TeamRules.Add(new TeamRule { Id = ruleId, TeamSettingsId = settings.Id, Code = "CODE", Name = "Rule" });
        context.TeamRuleParameters.Add(new TeamRuleParameter { Id = parameterId, TeamRuleId = ruleId, Key = "Key", Value = "Value" });
        context.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = adminId, Role = ETeamMemberRole.Admin, Status = ETeamMemberStatus.Active });
        await context.SaveChangesAsync();

        var handler = new TeamSettingsHandler(context);

        var result = await handler.RemoveRuleParameterAsync(new RemoveTeamRuleParameterCommand(
            parameterId,
            adminId,
            false),
            CancellationToken.None);

        Assert.Equal(ETeamRuleParameterOperationStatus.Success, result.Status);
        Assert.Empty(context.TeamRuleParameters);
    }

    private static Team CreateTeam(Guid teamId)
        => new()
        {
            Id = teamId,
            OwnerUserId = Guid.NewGuid(),
            Name = "Time",
            HomeFieldName = "Campo"
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

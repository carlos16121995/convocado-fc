using ConvocadoFc.Domain.Models.Modules.Subscriptions;
using ConvocadoFc.Domain.Models.Modules.Teams;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ConvocadoFc.Infrastructure.Tests.Persistence;

public sealed class AppDbContextModelTests
{
    [Fact]
    public void Model_MapsSchemasAndTables()
    {
        using var context = CreateContext();

        var plan = context.Model.FindEntityType(typeof(Plan));
        Assert.Equal("Plans", plan!.GetTableName());
        Assert.Equal("subscriptions", plan!.GetSchema());

        var team = context.Model.FindEntityType(typeof(Team));
        Assert.Equal("Teams", team!.GetTableName());
        Assert.Equal("teams", team!.GetSchema());

        var user = context.Model.FindEntityType(typeof(ApplicationUser));
        Assert.Equal("AspNetUsers", user!.GetTableName());
        Assert.Equal("users", user!.GetSchema());
    }

    [Fact]
    public void Model_EnforcesLengthsAndRequiredFields()
    {
        using var context = CreateContext();

        var plan = context.Model.FindEntityType(typeof(Plan))!;
        Assert.Equal(120, plan.FindProperty(nameof(Plan.Name))!.GetMaxLength());
        Assert.False(plan.FindProperty(nameof(Plan.Name))!.IsNullable);
        Assert.Equal(50, plan.FindProperty(nameof(Plan.Code))!.GetMaxLength());

        var team = context.Model.FindEntityType(typeof(Team))!;
        Assert.Equal(150, team.FindProperty(nameof(Team.Name))!.GetMaxLength());
        Assert.False(team.FindProperty(nameof(Team.Name))!.IsNullable);
        Assert.Equal(500, team.FindProperty(nameof(Team.CrestUrl))!.GetMaxLength());

        var entry = context.Model.FindEntityType(typeof(TeamSettingEntry))!;
        Assert.Equal(120, entry.FindProperty(nameof(TeamSettingEntry.Key))!.GetMaxLength());
        Assert.False(entry.FindProperty(nameof(TeamSettingEntry.Key))!.IsNullable);
        Assert.Equal(1200, entry.FindProperty(nameof(TeamSettingEntry.Value))!.GetMaxLength());
    }

    [Fact]
    public void Model_DefinesUniqueIndexes()
    {
        using var context = CreateContext();

        var member = context.Model.FindEntityType(typeof(TeamMember))!;
        Assert.Contains(member.GetIndexes(), index => index.IsUnique && IndexMatches(index, nameof(TeamMember.TeamId), nameof(TeamMember.UserId)));

        var invite = context.Model.FindEntityType(typeof(TeamInvite))!;
        Assert.Contains(invite.GetIndexes(), index => index.IsUnique && IndexMatches(index, nameof(TeamInvite.Token)));

        var entry = context.Model.FindEntityType(typeof(TeamSettingEntry))!;
        Assert.Contains(entry.GetIndexes(), index => index.IsUnique && IndexMatches(index, nameof(TeamSettingEntry.TeamSettingsId), nameof(TeamSettingEntry.Key)));
    }

    private static bool IndexMatches(IIndex index, params string[] propertyNames)
        => index.Properties.Select(property => property.Name).SequenceEqual(propertyNames);

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }
}

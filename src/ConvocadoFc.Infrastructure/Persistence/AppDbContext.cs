using ConvocadoFc.Application.Abstractions;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Models.Modules.Notifications;
using ConvocadoFc.Domain.Models.Modules.Users;
using ConvocadoFc.Domain.Models.Modules.Subscriptions;
using ConvocadoFc.Domain.Models.Modules.Teams;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ConvocadoFc.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IApplicationDbContext
{
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionHistory> SubscriptionHistories => Set<SubscriptionHistory>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<TeamMemberProfile> TeamMemberProfiles => Set<TeamMemberProfile>();
    public DbSet<TeamInvite> TeamInvites => Set<TeamInvite>();
    public DbSet<TeamJoinRequest> TeamJoinRequests => Set<TeamJoinRequest>();
    public DbSet<TeamSettings> TeamSettings => Set<TeamSettings>();
    public DbSet<TeamSettingEntry> TeamSettingEntries => Set<TeamSettingEntry>();
    public DbSet<TeamRule> TeamRules => Set<TeamRule>();
    public DbSet<TeamRuleParameter> TeamRuleParameters => Set<TeamRuleParameter>();

    public IQueryable<TEntity> Query<TEntity>() where TEntity : class
        => Set<TEntity>().AsNoTracking();

    public IQueryable<TEntity> Track<TEntity>() where TEntity : class
        => Set<TEntity>();

    async Task IApplicationDbContext.AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
        => await Set<TEntity>().AddAsync(entity, cancellationToken);

    Task IApplicationDbContext.UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
    {
        Set<TEntity>().Update(entity);
        return Task.CompletedTask;
    }

    Task IApplicationDbContext.RemoveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
    {
        Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }

    async Task IApplicationDbContext.ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
        await action(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("AspNetUsers", UsersSchema.Name);
        builder.Entity<ApplicationRole>().ToTable("AspNetRoles", UsersSchema.Name);
        builder.Entity<IdentityUserRole<Guid>>().ToTable("AspNetUserRoles", UsersSchema.Name);
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("AspNetUserClaims", UsersSchema.Name);
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("AspNetUserLogins", UsersSchema.Name);
        builder.Entity<IdentityUserToken<Guid>>().ToTable("AspNetUserTokens", UsersSchema.Name);
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("AspNetRoleClaims", UsersSchema.Name);

        builder.Entity<NotificationLog>().ToTable("NotificationLogs", NotificationsSchema.Name);

        builder.Entity<Plan>().ToTable("Plans", SubscriptionsSchema.Name);
        builder.Entity<Subscription>().ToTable("Subscriptions", SubscriptionsSchema.Name);
        builder.Entity<SubscriptionHistory>().ToTable("SubscriptionHistories", SubscriptionsSchema.Name);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FullName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(user => user.Address)
                .HasMaxLength(300);

            entity.Property(user => user.ProfilePhotoUrl)
                .HasMaxLength(500);

            entity.Property(user => user.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(user => user.Email)
                .HasMaxLength(320)
                .IsRequired();
        });

        builder.Entity<Plan>(entity =>
        {
            entity.HasIndex(plan => plan.Code).IsUnique();

            entity.Property(plan => plan.Name)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(plan => plan.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(plan => plan.Currency)
                .HasMaxLength(10)
                .IsRequired();

            entity.HasMany(plan => plan.Subscriptions)
                .WithOne(subscription => subscription.Plan)
                .HasForeignKey(subscription => subscription.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Subscription>(entity =>
        {
            entity.HasIndex(subscription => subscription.OwnerUserId);
            entity.HasIndex(subscription => subscription.PlanId);
            entity.HasIndex(subscription => new { subscription.OwnerUserId, subscription.Status })
                .HasFilter("\"Status\" = 1");

            entity.Property(subscription => subscription.Notes)
                .HasMaxLength(500);

            entity.HasOne(subscription => subscription.OwnerUser)
                .WithMany()
                .HasForeignKey(subscription => subscription.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(subscription => subscription.Histories)
                .WithOne(history => history.Subscription)
                .HasForeignKey(history => history.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SubscriptionHistory>(entity =>
        {
            entity.HasIndex(history => history.SubscriptionId);
            entity.HasIndex(history => history.OwnerUserId);

            entity.Property(history => history.Note)
                .HasMaxLength(500);

            entity.HasOne(history => history.OldPlan)
                .WithMany()
                .HasForeignKey(history => history.OldPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(history => history.NewPlan)
                .WithMany()
                .HasForeignKey(history => history.NewPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(history => history.OwnerUser)
                .WithMany()
                .HasForeignKey(history => history.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(history => history.ChangedByUser)
                .WithMany()
                .HasForeignKey(history => history.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Team>().ToTable("Teams", TeamsSchema.Name);
        builder.Entity<TeamMember>().ToTable("TeamMembers", TeamsSchema.Name);
        builder.Entity<TeamMemberProfile>().ToTable("TeamMemberProfiles", TeamsSchema.Name);
        builder.Entity<TeamInvite>().ToTable("TeamInvites", TeamsSchema.Name);
        builder.Entity<TeamJoinRequest>().ToTable("TeamJoinRequests", TeamsSchema.Name);
        builder.Entity<TeamSettings>().ToTable("TeamSettings", TeamsSchema.Name);
        builder.Entity<TeamSettingEntry>().ToTable("TeamSettingEntries", TeamsSchema.Name);
        builder.Entity<TeamRule>().ToTable("TeamRules", TeamsSchema.Name);
        builder.Entity<TeamRuleParameter>().ToTable("TeamRuleParameters", TeamsSchema.Name);

        builder.Entity<Team>(entity =>
        {
            entity.HasIndex(team => team.OwnerUserId);

            entity.Property(team => team.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(team => team.HomeFieldName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(team => team.HomeFieldAddress)
                .HasMaxLength(300);

            entity.Property(team => team.CrestUrl)
                .HasMaxLength(500);

            entity.HasOne(team => team.OwnerUser)
                .WithMany()
                .HasForeignKey(team => team.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(team => team.Settings)
                .WithOne(settings => settings.Team)
                .HasForeignKey<TeamSettings>(settings => settings.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TeamMember>(entity =>
        {
            entity.HasIndex(member => new { member.TeamId, member.UserId }).IsUnique();
            entity.HasIndex(member => member.TeamId);
            entity.HasIndex(member => member.UserId);

            entity.HasOne(member => member.Team)
                .WithMany(team => team.Members)
                .HasForeignKey(member => member.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(member => member.User)
                .WithMany()
                .HasForeignKey(member => member.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(member => member.AddedByUser)
                .WithMany()
                .HasForeignKey(member => member.AddedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TeamMemberProfile>(entity =>
        {
            entity.HasIndex(profile => profile.TeamMemberId).IsUnique();

            entity.HasOne(profile => profile.TeamMember)
                .WithOne(member => member.Profile)
                .HasForeignKey<TeamMemberProfile>(profile => profile.TeamMemberId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TeamInvite>(entity =>
        {
            entity.HasIndex(invite => invite.TeamId);
            entity.HasIndex(invite => invite.CreatedByUserId);
            entity.HasIndex(invite => invite.Status);
            entity.HasIndex(invite => invite.Token).IsUnique();

            entity.Property(invite => invite.TargetEmail)
                .HasMaxLength(320);

            entity.Property(invite => invite.Token)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(invite => invite.Message)
                .HasMaxLength(500);

            entity.HasOne(invite => invite.Team)
                .WithMany(team => team.Invites)
                .HasForeignKey(invite => invite.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(invite => invite.CreatedByUser)
                .WithMany()
                .HasForeignKey(invite => invite.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(invite => invite.TargetUser)
                .WithMany()
                .HasForeignKey(invite => invite.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TeamJoinRequest>(entity =>
        {
            entity.HasIndex(request => request.TeamId);
            entity.HasIndex(request => request.UserId);
            entity.HasIndex(request => request.Status);

            entity.Property(request => request.Message)
                .HasMaxLength(500);

            entity.HasOne(request => request.Team)
                .WithMany(team => team.JoinRequests)
                .HasForeignKey(request => request.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(request => request.User)
                .WithMany()
                .HasForeignKey(request => request.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(request => request.ReviewedByUser)
                .WithMany()
                .HasForeignKey(request => request.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(request => request.Invite)
                .WithMany()
                .HasForeignKey(request => request.InviteId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<TeamSettings>(entity =>
        {
            entity.HasIndex(settings => settings.TeamId).IsUnique();

            entity.HasOne(settings => settings.Team)
                .WithOne(team => team.Settings)
                .HasForeignKey<TeamSettings>(settings => settings.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TeamSettingEntry>(entity =>
        {
            entity.HasIndex(entry => new { entry.TeamSettingsId, entry.Key }).IsUnique();

            entity.Property(entry => entry.Key)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(entry => entry.Value)
                .HasMaxLength(1200)
                .IsRequired();

            entity.Property(entry => entry.ValueType)
                .HasMaxLength(40);

            entity.Property(entry => entry.Description)
                .HasMaxLength(300);

            entity.HasOne(entry => entry.TeamSettings)
                .WithMany(settings => settings.Settings)
                .HasForeignKey(entry => entry.TeamSettingsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TeamRule>(entity =>
        {
            entity.HasIndex(rule => rule.TeamSettingsId);
            entity.HasIndex(rule => rule.Code);

            entity.Property(rule => rule.Code)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(rule => rule.Name)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(rule => rule.Description)
                .HasMaxLength(500);

            entity.Property(rule => rule.Scope)
                .HasMaxLength(80);

            entity.Property(rule => rule.Target)
                .HasMaxLength(120);

            entity.HasOne(rule => rule.TeamSettings)
                .WithMany(settings => settings.Rules)
                .HasForeignKey(rule => rule.TeamSettingsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TeamRuleParameter>(entity =>
        {
            entity.HasIndex(parameter => new { parameter.TeamRuleId, parameter.Key }).IsUnique();

            entity.Property(parameter => parameter.Key)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(parameter => parameter.Value)
                .HasMaxLength(1200)
                .IsRequired();

            entity.Property(parameter => parameter.ValueType)
                .HasMaxLength(40);

            entity.Property(parameter => parameter.Unit)
                .HasMaxLength(40);

            entity.Property(parameter => parameter.Description)
                .HasMaxLength(300);

            entity.HasOne(parameter => parameter.TeamRule)
                .WithMany(rule => rule.Parameters)
                .HasForeignKey(parameter => parameter.TeamRuleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

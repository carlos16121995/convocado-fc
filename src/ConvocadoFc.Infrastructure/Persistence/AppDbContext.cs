using ConvocadoFc.Application.Abstractions;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Models.Modules.Notifications;
using ConvocadoFc.Domain.Models.Modules.Users;
using ConvocadoFc.Domain.Models.Modules.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConvocadoFc.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IApplicationDbContext
{
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionHistory> SubscriptionHistories => Set<SubscriptionHistory>();

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
    }
}

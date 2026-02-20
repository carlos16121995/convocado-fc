using ConvocadoFc.Application.Abstractions;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Models.Modules.Notifications;
using ConvocadoFc.Domain.Models.Modules.Users;
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

    public IQueryable<TEntity> Query<TEntity>() where TEntity : class
        => Set<TEntity>().AsNoTracking();

    async Task IApplicationDbContext.AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
        => await Set<TEntity>().AddAsync(entity, cancellationToken);

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
    }
}

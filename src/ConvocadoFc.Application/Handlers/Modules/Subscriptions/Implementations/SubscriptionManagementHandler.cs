using ConvocadoFc.Application.Abstractions;
using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;
using ConvocadoFc.Domain.Models.Modules.Subscriptions;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Implementations;

public sealed class SubscriptionManagementHandler(
    IApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager) : ISubscriptionManagementHandler
{
    private readonly IApplicationDbContext _dbContext = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<IReadOnlyCollection<SubscriptionDto>> ListSubscriptionsAsync(ListSubscriptionsQuery query, CancellationToken cancellationToken)
    {
        var subscriptionsQuery = ApplySubscriptionFilters(_dbContext.Query<Subscription>(), query);

        var resultQuery = from subscription in subscriptionsQuery
                          join plan in _dbContext.Query<Plan>() on subscription.PlanId equals plan.Id
                          orderby subscription.StartsAt descending
                          select new { subscription, plan };

        return await resultQuery
            .Select(data => MapToDto(data.subscription, data.plan))
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedResult<SubscribedUserDto>> ListSubscribedUsersAsync(ListSubscribedUsersQuery query, CancellationToken cancellationToken)
    {
        var subscriptionsQuery = ApplySubscriptionFilters(_dbContext.Query<Subscription>(), new ListSubscriptionsQuery(null, query.Status, null));

        var latestSubscriptions = subscriptionsQuery
            .GroupBy(subscription => subscription.OwnerUserId)
            .Select(group => group.OrderByDescending(subscription => subscription.StartsAt).FirstOrDefault()!);

        var usersQuery = from subscription in latestSubscriptions
                         join user in _dbContext.Query<ApplicationUser>() on subscription.OwnerUserId equals user.Id
                         join plan in _dbContext.Query<Plan>() on subscription.PlanId equals plan.Id
                         select new { subscription, user, plan };

        var totalItems = await usersQuery.CountAsync(cancellationToken);

        var items = await usersQuery
            .OrderBy(entry => entry.user.FullName)
            .Skip((query.Pagination.Page - 1) * query.Pagination.PageSize)
            .Take(query.Pagination.PageSize)
            .Select(entry => new SubscribedUserDto(
                entry.user.Id,
                entry.user.Email ?? string.Empty,
                entry.user.FullName,
                MapToDto(entry.subscription, entry.plan)))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<SubscribedUserDto>
        {
            TotalItems = totalItems,
            Items = items
        };
    }

    public async Task<SubscriptionOperationResult> AssignSubscriptionAsync(AssignSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.OwnerUserId.ToString());
        if (user is null)
        {
            return new SubscriptionOperationResult(ESubscriptionOperationStatus.UserNotFound, null);
        }

        var plan = await _dbContext.Query<Plan>()
            .FirstOrDefaultAsync(existing => existing.Id == command.PlanId && existing.IsActive, cancellationToken);
        if (plan is null)
        {
            return new SubscriptionOperationResult(ESubscriptionOperationStatus.PlanNotFound, null);
        }

        var hasActiveSubscription = await _dbContext.Query<Subscription>()
            .AnyAsync(subscription => subscription.OwnerUserId == command.OwnerUserId
                                      && subscription.Status == ESubscriptionStatus.Active,
                cancellationToken);

        if (hasActiveSubscription)
        {
            return new SubscriptionOperationResult(ESubscriptionOperationStatus.ActiveSubscriptionExists, null);
        }

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            OwnerUserId = command.OwnerUserId,
            PlanId = plan.Id,
            Status = ESubscriptionStatus.Active,
            StartsAt = command.StartsAt ?? DateTimeOffset.UtcNow,
            EndsAt = command.EndsAt,
            AutoRenew = command.AutoRenew,
            CreatedAt = DateTimeOffset.UtcNow,
            Notes = command.Note
        };

        var history = new SubscriptionHistory
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            OwnerUserId = subscription.OwnerUserId,
            OldPlanId = null,
            NewPlanId = subscription.PlanId,
            OldStatus = null,
            NewStatus = subscription.Status,
            Action = ESubscriptionHistoryAction.Assigned,
            ChangedByUserId = command.AssignedByUserId,
            OccurredAt = DateTimeOffset.UtcNow,
            Note = command.Note
        };

        await _dbContext.ExecuteInTransactionAsync(async token =>
        {
            await _dbContext.AddAsync(subscription, token);
            await _dbContext.AddAsync(history, token);
            await _dbContext.SaveChangesAsync(token);
        }, cancellationToken);

        return new SubscriptionOperationResult(ESubscriptionOperationStatus.Success, MapToDto(subscription, plan));
    }

    public async Task<SubscriptionOperationResult> ChangeSubscriptionAsync(ChangeSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var subscription = await _dbContext.Track<Subscription>()
            .FirstOrDefaultAsync(existing => existing.Id == command.SubscriptionId, cancellationToken);

        if (subscription is null)
        {
            return new SubscriptionOperationResult(ESubscriptionOperationStatus.NotFound, null);
        }

        Plan? plan = null;
        if (command.PlanId.HasValue)
        {
            plan = await _dbContext.Query<Plan>()
                .FirstOrDefaultAsync(existing => existing.Id == command.PlanId && existing.IsActive, cancellationToken);

            if (plan is null)
            {
                return new SubscriptionOperationResult(ESubscriptionOperationStatus.PlanNotFound, null);
            }
        }

        var oldPlanId = subscription.PlanId;
        var oldStatus = subscription.Status;

        if (command.PlanId.HasValue)
        {
            subscription.PlanId = command.PlanId.Value;
        }

        if (command.EndsAt.HasValue)
        {
            subscription.EndsAt = command.EndsAt;
        }

        if (command.AutoRenew.HasValue)
        {
            subscription.AutoRenew = command.AutoRenew.Value;
        }

        if (command.Status.HasValue)
        {
            subscription.Status = command.Status.Value;
        }

        if (subscription.Status == ESubscriptionStatus.Canceled)
        {
            subscription.CanceledAt ??= DateTimeOffset.UtcNow;
            subscription.EndsAt ??= DateTimeOffset.UtcNow;
        }

        subscription.UpdatedAt = DateTimeOffset.UtcNow;

        var history = new SubscriptionHistory
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            OwnerUserId = subscription.OwnerUserId,
            OldPlanId = oldPlanId,
            NewPlanId = subscription.PlanId,
            OldStatus = oldStatus,
            NewStatus = subscription.Status,
            Action = ESubscriptionHistoryAction.Changed,
            ChangedByUserId = command.ChangedByUserId,
            OccurredAt = DateTimeOffset.UtcNow,
            Note = command.Note
        };

        await _dbContext.ExecuteInTransactionAsync(async token =>
        {
            await _dbContext.AddAsync(history, token);
            await _dbContext.SaveChangesAsync(token);
        }, cancellationToken);

        plan ??= await _dbContext.Query<Plan>()
            .FirstOrDefaultAsync(existing => existing.Id == subscription.PlanId, cancellationToken);

        if (plan is null)
        {
            return new SubscriptionOperationResult(ESubscriptionOperationStatus.PlanNotFound, null);
        }

        return new SubscriptionOperationResult(ESubscriptionOperationStatus.Success, MapToDto(subscription, plan));
    }

    public async Task<SubscriptionOperationResult> RemoveSubscriptionAsync(RemoveSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var subscription = await _dbContext.Track<Subscription>()
            .FirstOrDefaultAsync(existing => existing.Id == command.SubscriptionId, cancellationToken);

        if (subscription is null)
        {
            return new SubscriptionOperationResult(ESubscriptionOperationStatus.NotFound, null);
        }

        if (subscription.Status != ESubscriptionStatus.Active)
        {
            return new SubscriptionOperationResult(ESubscriptionOperationStatus.SubscriptionNotActive, null);
        }

        var oldStatus = subscription.Status;
        var oldPlanId = subscription.PlanId;

        subscription.Status = ESubscriptionStatus.Canceled;
        subscription.CanceledAt = DateTimeOffset.UtcNow;
        subscription.EndsAt ??= DateTimeOffset.UtcNow;
        subscription.UpdatedAt = DateTimeOffset.UtcNow;

        var history = new SubscriptionHistory
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            OwnerUserId = subscription.OwnerUserId,
            OldPlanId = oldPlanId,
            NewPlanId = subscription.PlanId,
            OldStatus = oldStatus,
            NewStatus = subscription.Status,
            Action = ESubscriptionHistoryAction.Removed,
            ChangedByUserId = command.RemovedByUserId,
            OccurredAt = DateTimeOffset.UtcNow,
            Note = command.Note
        };

        await _dbContext.ExecuteInTransactionAsync(async token =>
        {
            await _dbContext.AddAsync(history, token);
            await _dbContext.SaveChangesAsync(token);
        }, cancellationToken);

        var plan = await _dbContext.Query<Plan>()
            .FirstOrDefaultAsync(existing => existing.Id == subscription.PlanId, cancellationToken);

        if (plan is null)
        {
            return new SubscriptionOperationResult(ESubscriptionOperationStatus.PlanNotFound, null);
        }

        return new SubscriptionOperationResult(ESubscriptionOperationStatus.Success, MapToDto(subscription, plan));
    }

    private static IQueryable<Subscription> ApplySubscriptionFilters(IQueryable<Subscription> query, ListSubscriptionsQuery filters)
    {
        if (filters.OwnerUserId.HasValue)
        {
            query = query.Where(subscription => subscription.OwnerUserId == filters.OwnerUserId.Value);
        }

        if (filters.Status.HasValue)
        {
            query = query.Where(subscription => subscription.Status == filters.Status.Value);
        }

        if (filters.PlanId.HasValue)
        {
            query = query.Where(subscription => subscription.PlanId == filters.PlanId.Value);
        }

        return query;
    }

    private static SubscriptionDto MapToDto(Subscription subscription, Plan plan)
        => new SubscriptionDto(
            subscription.Id,
            subscription.OwnerUserId,
            plan.Id,
            plan.Name,
            plan.Code,
            subscription.Status,
            subscription.StartsAt,
            subscription.EndsAt,
            subscription.AutoRenew,
            new PlanCapacityDto(plan.MaxTeams, plan.MaxMembersPerTeam));
}

using ConvocadoFc.Application.Abstractions;
using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Implementations;
using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;
using ConvocadoFc.Domain.Models.Modules.Subscriptions;
using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Domain.Shared;
using ConvocadoFc.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;
using System.Reflection;

namespace ConvocadoFc.Application.Tests;

public sealed class SubscriptionManagementHandlerTests
{
    [Fact]
    public async Task ListSubscriptionsAsync_WhenFiltered_ReturnsOrderedResults()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var otherPlanId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Plans.AddRange(
            CreatePlan(planId, "Plano A", "PLAN_A"),
            CreatePlan(otherPlanId, "Plano B", "PLAN_B"));
        context.Subscriptions.AddRange(
            CreateSubscription(Guid.NewGuid(), ownerId, planId, ESubscriptionStatus.Active, new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            CreateSubscription(Guid.NewGuid(), ownerId, planId, ESubscriptionStatus.Active, new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero)),
            CreateSubscription(Guid.NewGuid(), ownerId, otherPlanId, ESubscriptionStatus.Canceled, new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero)),
            CreateSubscription(Guid.NewGuid(), otherUserId, planId, ESubscriptionStatus.Active, new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero)));
        await context.SaveChangesAsync();

        var handler = new SubscriptionManagementHandler(context, CreateUserManager().Object);

        var result = await handler.ListSubscriptionsAsync(new ListSubscriptionsQuery(
            ownerId,
            ESubscriptionStatus.Active,
            planId),
            CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero), result.First().StartsAt);
        Assert.Equal("Plano A", result.First().PlanName);
        Assert.Equal("PLAN_A", result.First().PlanCode);
    }

    [Fact]
    public async Task ListSubscribedUsersAsync_WhenActive_ReturnsLatestPerUser()
    {
        var planId = Guid.NewGuid();
        var userAnaId = Guid.NewGuid();
        var userBrunoId = Guid.NewGuid();

        var context = new FakeApplicationDbContext(
            new[] { CreatePlan(planId, "Plano", "PLAN") },
            new[]
            {
                CreateUser(userAnaId, "ana@local", "Ana"),
                CreateUser(userBrunoId, "bruno@local", "Bruno")
            },
            new[]
            {
                CreateSubscription(Guid.NewGuid(), userAnaId, planId, ESubscriptionStatus.Active, new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)),
                CreateSubscription(Guid.NewGuid(), userAnaId, planId, ESubscriptionStatus.Active, new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero)),
                CreateSubscription(Guid.NewGuid(), userBrunoId, planId, ESubscriptionStatus.Active, new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero))
            });

        var handler = new SubscriptionManagementHandler(context, CreateUserManager().Object);

        var result = await handler.ListSubscribedUsersAsync(new ListSubscribedUsersQuery(
            new PaginationQuery { Page = 1, PageSize = 10 },
            ESubscriptionStatus.Active),
            CancellationToken.None);

        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Ana", result.Items[0].FullName);
        Assert.Equal(new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero), result.Items[0].Subscription.StartsAt);
        Assert.Equal("Bruno", result.Items[1].FullName);
    }

    [Fact]
    public async Task AssignSubscriptionAsync_WhenUserNotFound_ReturnsUserNotFound()
    {
        await using var context = CreateContext();
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new SubscriptionManagementHandler(context, userManager.Object);

        var result = await handler.AssignSubscriptionAsync(new AssignSubscriptionCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            false,
            Guid.NewGuid(),
            null),
            CancellationToken.None);

        Assert.Equal(ESubscriptionOperationStatus.UserNotFound, result.Status);
        Assert.Null(result.Subscription);
    }

    [Fact]
    public async Task AssignSubscriptionAsync_WhenPlanNotFound_ReturnsPlanNotFound()
    {
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Plans.Add(new Plan
        {
            Id = planId,
            Name = "Plano",
            Code = "PLAN",
            Currency = "BRL",
            IsActive = false
        });
        await context.SaveChangesAsync();

        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(CreateUser(userId, "user@local", "User"));

        var handler = new SubscriptionManagementHandler(context, userManager.Object);

        var result = await handler.AssignSubscriptionAsync(new AssignSubscriptionCommand(
            userId,
            planId,
            null,
            null,
            false,
            Guid.NewGuid(),
            null),
            CancellationToken.None);

        Assert.Equal(ESubscriptionOperationStatus.PlanNotFound, result.Status);
        Assert.Null(result.Subscription);
    }

    [Fact]
    public async Task AssignSubscriptionAsync_WhenActiveSubscriptionExists_ReturnsConflict()
    {
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Plans.Add(CreatePlan(planId, "Plano", "PLAN"));
        context.Subscriptions.Add(CreateSubscription(Guid.NewGuid(), userId, planId, ESubscriptionStatus.Active, DateTimeOffset.UtcNow));
        await context.SaveChangesAsync();

        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(CreateUser(userId, "user@local", "User"));

        var handler = new SubscriptionManagementHandler(context, userManager.Object);

        var result = await handler.AssignSubscriptionAsync(new AssignSubscriptionCommand(
            userId,
            planId,
            null,
            null,
            false,
            Guid.NewGuid(),
            null),
            CancellationToken.None);

        Assert.Equal(ESubscriptionOperationStatus.ActiveSubscriptionExists, result.Status);
        Assert.Null(result.Subscription);
    }

    [Fact]
    public async Task AssignSubscriptionAsync_WhenSuccessful_CreatesSubscriptionAndHistory()
    {
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var startsAt = new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero);

        await using var context = CreateContext();
        context.Plans.Add(CreatePlan(planId, "Plano", "PLAN"));
        await context.SaveChangesAsync();

        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(CreateUser(userId, "user@local", "User"));

        var handler = new SubscriptionManagementHandler(context, userManager.Object);

        var result = await handler.AssignSubscriptionAsync(new AssignSubscriptionCommand(
            userId,
            planId,
            startsAt,
            null,
            true,
            Guid.NewGuid(),
            "note"),
            CancellationToken.None);

        Assert.Equal(ESubscriptionOperationStatus.Success, result.Status);
        Assert.NotNull(result.Subscription);
        Assert.Equal(userId, result.Subscription!.OwnerUserId);
        Assert.Equal(planId, result.Subscription.PlanId);
        Assert.Equal(startsAt, result.Subscription.StartsAt);
        Assert.True(result.Subscription.AutoRenew);

        Assert.Single(context.Subscriptions);
        Assert.Single(context.SubscriptionHistories);
    }

    [Fact]
    public async Task ChangeSubscriptionAsync_WhenNotFound_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var handler = new SubscriptionManagementHandler(context, CreateUserManager().Object);

        var result = await handler.ChangeSubscriptionAsync(new ChangeSubscriptionCommand(
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            Guid.NewGuid(),
            null),
            CancellationToken.None);

        Assert.Equal(ESubscriptionOperationStatus.NotFound, result.Status);
        Assert.Null(result.Subscription);
    }

    [Fact]
    public async Task ChangeSubscriptionAsync_WhenPlanNotFound_ReturnsPlanNotFound()
    {
        var subscriptionId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Subscriptions.Add(CreateSubscription(subscriptionId, ownerId, Guid.NewGuid(), ESubscriptionStatus.Active, DateTimeOffset.UtcNow));
        await context.SaveChangesAsync();

        var handler = new SubscriptionManagementHandler(context, CreateUserManager().Object);

        var result = await handler.ChangeSubscriptionAsync(new ChangeSubscriptionCommand(
            subscriptionId,
            Guid.NewGuid(),
            null,
            null,
            null,
            Guid.NewGuid(),
            null),
            CancellationToken.None);

        Assert.Equal(ESubscriptionOperationStatus.PlanNotFound, result.Status);
        Assert.Null(result.Subscription);
    }

    [Fact]
    public async Task ChangeSubscriptionAsync_WhenCanceled_UpdatesSubscriptionAndHistory()
    {
        var subscriptionId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var newPlanId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Plans.AddRange(
            CreatePlan(planId, "Plano A", "PLAN_A"),
            CreatePlan(newPlanId, "Plano B", "PLAN_B"));
        context.Subscriptions.Add(CreateSubscription(subscriptionId, ownerId, planId, ESubscriptionStatus.Active, DateTimeOffset.UtcNow));
        await context.SaveChangesAsync();

        var handler = new SubscriptionManagementHandler(context, CreateUserManager().Object);

        var result = await handler.ChangeSubscriptionAsync(new ChangeSubscriptionCommand(
            subscriptionId,
            newPlanId,
            null,
            true,
            ESubscriptionStatus.Canceled,
            Guid.NewGuid(),
            "note"),
            CancellationToken.None);

        Assert.Equal(ESubscriptionOperationStatus.Success, result.Status);
        Assert.NotNull(result.Subscription);
        Assert.Equal(newPlanId, result.Subscription!.PlanId);
        Assert.Equal(ESubscriptionStatus.Canceled, result.Subscription.Status);
        Assert.True(result.Subscription.AutoRenew);
        Assert.NotNull(result.Subscription.EndsAt);

        var updatedSubscription = await context.Subscriptions.FirstAsync(item => item.Id == subscriptionId);
        Assert.NotNull(updatedSubscription.CanceledAt);

        Assert.Single(context.SubscriptionHistories);
    }

    [Fact]
    public async Task RemoveSubscriptionAsync_WhenNotFound_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var handler = new SubscriptionManagementHandler(context, CreateUserManager().Object);

        var result = await handler.RemoveSubscriptionAsync(new RemoveSubscriptionCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null),
            CancellationToken.None);

        Assert.Equal(ESubscriptionOperationStatus.NotFound, result.Status);
        Assert.Null(result.Subscription);
    }

    [Fact]
    public async Task RemoveSubscriptionAsync_WhenNotActive_ReturnsSubscriptionNotActive()
    {
        var subscriptionId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Subscriptions.Add(CreateSubscription(subscriptionId, ownerId, Guid.NewGuid(), ESubscriptionStatus.Canceled, DateTimeOffset.UtcNow));
        await context.SaveChangesAsync();

        var handler = new SubscriptionManagementHandler(context, CreateUserManager().Object);

        var result = await handler.RemoveSubscriptionAsync(new RemoveSubscriptionCommand(
            subscriptionId,
            Guid.NewGuid(),
            null),
            CancellationToken.None);

        Assert.Equal(ESubscriptionOperationStatus.SubscriptionNotActive, result.Status);
        Assert.Null(result.Subscription);
    }

    [Fact]
    public async Task RemoveSubscriptionAsync_WhenSuccessful_CancelsAndCreatesHistory()
    {
        var subscriptionId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        await using var context = CreateContext();
        context.Plans.Add(CreatePlan(planId, "Plano", "PLAN"));
        context.Subscriptions.Add(CreateSubscription(subscriptionId, ownerId, planId, ESubscriptionStatus.Active, DateTimeOffset.UtcNow));
        await context.SaveChangesAsync();

        var handler = new SubscriptionManagementHandler(context, CreateUserManager().Object);

        var result = await handler.RemoveSubscriptionAsync(new RemoveSubscriptionCommand(
            subscriptionId,
            Guid.NewGuid(),
            "note"),
            CancellationToken.None);

        Assert.Equal(ESubscriptionOperationStatus.Success, result.Status);
        Assert.NotNull(result.Subscription);
        Assert.Equal(ESubscriptionStatus.Canceled, result.Subscription!.Status);
        Assert.NotNull(result.Subscription.EndsAt);

        var updatedSubscription = await context.Subscriptions.FirstAsync(item => item.Id == subscriptionId);
        Assert.NotNull(updatedSubscription.CanceledAt);

        Assert.Single(context.SubscriptionHistories);
    }

    private static Subscription CreateSubscription(Guid id, Guid ownerUserId, Guid planId, ESubscriptionStatus status, DateTimeOffset startsAt)
        => new()
        {
            Id = id,
            OwnerUserId = ownerUserId,
            PlanId = planId,
            Status = status,
            StartsAt = startsAt,
            AutoRenew = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

    private static Plan CreatePlan(Guid id, string name, string code)
        => new()
        {
            Id = id,
            Name = name,
            Code = code,
            Currency = "BRL",
            IsActive = true,
            MaxTeams = 2,
            MaxMembersPerTeam = 10
        };

    private static ApplicationUser CreateUser(Guid userId, string email, string name)
        => new()
        {
            Id = userId,
            Email = email,
            FullName = name,
            PhoneNumber = "11999999999"
        };

    private static Mock<UserManager<ApplicationUser>> CreateUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }

    private sealed class FakeApplicationDbContext : IApplicationDbContext
    {
        private readonly IReadOnlyDictionary<Type, IQueryable> _data;

        public FakeApplicationDbContext(IEnumerable<Plan> plans, IEnumerable<ApplicationUser> users, IEnumerable<Subscription> subscriptions)
        {
            _data = new Dictionary<Type, IQueryable>
            {
                [typeof(Plan)] = new TestAsyncEnumerable<Plan>(plans),
                [typeof(ApplicationUser)] = new TestAsyncEnumerable<ApplicationUser>(users),
                [typeof(Subscription)] = new TestAsyncEnumerable<Subscription>(subscriptions)
            };
        }

        public IQueryable<TEntity> Query<TEntity>() where TEntity : class
        {
            if (_data.TryGetValue(typeof(TEntity), out var query))
            {
                return (IQueryable<TEntity>)query;
            }

            return new TestAsyncEnumerable<TEntity>(Array.Empty<TEntity>());
        }

        public IQueryable<TEntity> Track<TEntity>() where TEntity : class
            => Query<TEntity>();

        public Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
            => throw new NotSupportedException();

        public Task UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
            => throw new NotSupportedException();

        public Task RemoveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
            => throw new NotSupportedException();

        public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(0);
    }

    private sealed class TestAsyncQueryProvider<TEntity>(IQueryProvider inner) : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner = inner;

        public IQueryable CreateQuery(Expression expression)
            => new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new TestAsyncEnumerable<TElement>(expression);

        public object Execute(Expression expression)
            => _inner.Execute(expression)!;

        public TResult Execute<TResult>(Expression expression)
            => _inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            var resultType = typeof(TResult);
            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var innerType = resultType.GetGenericArguments()[0];
                var executeMethod = typeof(IQueryProvider)
                    .GetMethods()
                    .Single(method => method.Name == nameof(IQueryProvider.Execute)
                                      && method.IsGenericMethodDefinition
                                      && method.GetParameters().Length == 1)
                    .MakeGenericMethod(innerType);

                var result = executeMethod.Invoke(_inner, new object[] { expression });
                var fromResultMethod = typeof(Task)
                    .GetMethod(nameof(Task.FromResult))
                    ?.MakeGenericMethod(innerType);

                return (TResult)fromResultMethod!.Invoke(null, new[] { result })!;
            }

            return Execute<TResult>(expression);
        }
    }

    private sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        {
        }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

        IQueryProvider IQueryable.Provider
            => new TestAsyncQueryProvider<T>(this);
    }

    private sealed class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner = inner;

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
            => new(_inner.MoveNext());
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ConvocadoFc.Application.Abstractions;
using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Interfaces;
using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;
using ConvocadoFc.Domain.Models.Modules.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Implementations;

public sealed class PlanManagementHandler(IApplicationDbContext dbContext) : IPlanManagementHandler
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<IReadOnlyCollection<PlanDto>> ListPlansAsync(CancellationToken cancellationToken)
        => await _dbContext.Query<Plan>()
            .OrderBy(plan => plan.Name)
            .Select(plan => new PlanDto(
                plan.Id,
                plan.Name,
                plan.Code,
                plan.Price,
                plan.Currency,
                plan.IsActive,
                plan.IsCustomPricing,
                new PlanCapacityDto(plan.MaxTeams, plan.MaxMembersPerTeam)))
            .ToListAsync(cancellationToken);

    public async Task<PlanDto?> GetPlanAsync(Guid planId, CancellationToken cancellationToken)
        => await _dbContext.Query<Plan>()
            .Where(plan => plan.Id == planId)
            .Select(plan => new PlanDto(
                plan.Id,
                plan.Name,
                plan.Code,
                plan.Price,
                plan.Currency,
                plan.IsActive,
                plan.IsCustomPricing,
                new PlanCapacityDto(plan.MaxTeams, plan.MaxMembersPerTeam)))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<PlanOperationResult> CreatePlanAsync(CreatePlanCommand command, CancellationToken cancellationToken)
    {
        if (!IsCapacityValid(command.IsCustomPricing, command.MaxTeams, command.MaxMembersPerTeam))
        {
            return new PlanOperationResult(PlanOperationStatus.InvalidCapacity, null);
        }

        var normalizedCode = NormalizeCode(command.Code);
        var normalizedName = NormalizeName(command.Name);

        var codeExists = await _dbContext.Query<Plan>()
            .AnyAsync(plan => plan.Code == normalizedCode, cancellationToken);
        if (codeExists)
        {
            return new PlanOperationResult(PlanOperationStatus.CodeAlreadyExists, null);
        }

        var nameExists = await _dbContext.Query<Plan>()
            .AnyAsync(plan => plan.Name == normalizedName, cancellationToken);
        if (nameExists)
        {
            return new PlanOperationResult(PlanOperationStatus.NameAlreadyExists, null);
        }

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Code = normalizedCode,
            Price = command.Price,
            Currency = NormalizeCurrency(command.Currency),
            MaxTeams = command.MaxTeams,
            MaxMembersPerTeam = command.MaxMembersPerTeam,
            IsCustomPricing = command.IsCustomPricing,
            IsActive = command.IsActive,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _dbContext.AddAsync(plan, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PlanOperationResult(PlanOperationStatus.Success, MapToDto(plan));
    }

    public async Task<PlanOperationResult> UpdatePlanAsync(UpdatePlanCommand command, CancellationToken cancellationToken)
    {
        if (!IsCapacityValid(command.IsCustomPricing, command.MaxTeams, command.MaxMembersPerTeam))
        {
            return new PlanOperationResult(PlanOperationStatus.InvalidCapacity, null);
        }

        var plan = await _dbContext.Track<Plan>()
            .FirstOrDefaultAsync(existing => existing.Id == command.PlanId, cancellationToken);

        if (plan is null)
        {
            return new PlanOperationResult(PlanOperationStatus.NotFound, null);
        }

        var normalizedCode = NormalizeCode(command.Code);
        var normalizedName = NormalizeName(command.Name);

        var codeExists = await _dbContext.Query<Plan>()
            .AnyAsync(existing => existing.Code == normalizedCode && existing.Id != plan.Id, cancellationToken);
        if (codeExists)
        {
            return new PlanOperationResult(PlanOperationStatus.CodeAlreadyExists, null);
        }

        var nameExists = await _dbContext.Query<Plan>()
            .AnyAsync(existing => existing.Name == normalizedName && existing.Id != plan.Id, cancellationToken);
        if (nameExists)
        {
            return new PlanOperationResult(PlanOperationStatus.NameAlreadyExists, null);
        }

        plan.Name = normalizedName;
        plan.Code = normalizedCode;
        plan.Price = command.Price;
        plan.Currency = NormalizeCurrency(command.Currency);
        plan.MaxTeams = command.MaxTeams;
        plan.MaxMembersPerTeam = command.MaxMembersPerTeam;
        plan.IsCustomPricing = command.IsCustomPricing;
        plan.IsActive = command.IsActive;
        plan.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PlanOperationResult(PlanOperationStatus.Success, MapToDto(plan));
    }

    public async Task<PlanOperationResult> DeactivatePlanAsync(Guid planId, CancellationToken cancellationToken)
    {
        var plan = await _dbContext.Track<Plan>()
            .FirstOrDefaultAsync(existing => existing.Id == planId, cancellationToken);

        if (plan is null)
        {
            return new PlanOperationResult(PlanOperationStatus.NotFound, null);
        }

        plan.IsActive = false;
        plan.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PlanOperationResult(PlanOperationStatus.Success, MapToDto(plan));
    }

    private static bool IsCapacityValid(bool isCustomPricing, int? maxTeams, int? maxMembersPerTeam)
    {
        if (isCustomPricing)
        {
            return (!maxTeams.HasValue || maxTeams > 0) && (!maxMembersPerTeam.HasValue || maxMembersPerTeam > 0);
        }

        return maxTeams.HasValue && maxTeams > 0 && maxMembersPerTeam.HasValue && maxMembersPerTeam > 0;
    }

    private static string NormalizeCode(string code)
        => code.Trim().ToUpperInvariant();

    private static string NormalizeName(string name)
        => name.Trim();

    private static string NormalizeCurrency(string currency)
        => string.IsNullOrWhiteSpace(currency) ? "BRL" : currency.Trim().ToUpperInvariant();

    private static PlanDto MapToDto(Plan plan)
        => new PlanDto(
            plan.Id,
            plan.Name,
            plan.Code,
            plan.Price,
            plan.Currency,
            plan.IsActive,
            plan.IsCustomPricing,
            new PlanCapacityDto(plan.MaxTeams, plan.MaxMembersPerTeam));
}

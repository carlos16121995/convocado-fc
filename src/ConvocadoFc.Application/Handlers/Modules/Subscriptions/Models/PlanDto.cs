using System;

namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public sealed record PlanDto(
    Guid Id,
    string Name,
    string Code,
    decimal? Price,
    string Currency,
    bool IsActive,
    bool IsCustomPricing,
    PlanCapacityDto Capacity
);

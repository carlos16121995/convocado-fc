using System;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

public sealed record PlanResponse(
    Guid Id,
    string Name,
    string Code,
    decimal? Price,
    string Currency,
    bool IsActive,
    bool IsCustomPricing,
    PlanCapacityResponse Capacity
);

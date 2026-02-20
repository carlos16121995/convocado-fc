namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public sealed record PlanOperationResult(
    PlanOperationStatus Status,
    PlanDto? Plan
);

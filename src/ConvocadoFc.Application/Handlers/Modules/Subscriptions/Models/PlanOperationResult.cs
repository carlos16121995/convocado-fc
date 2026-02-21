namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public sealed record PlanOperationResult(
    EPlanOperationStatus Status,
    PlanDto? Plan
);

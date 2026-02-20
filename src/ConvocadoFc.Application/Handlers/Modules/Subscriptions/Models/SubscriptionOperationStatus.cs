namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public enum SubscriptionOperationStatus
{
    Success = 0,
    NotFound = 1,
    PlanNotFound = 2,
    UserNotFound = 3,
    ActiveSubscriptionExists = 4,
    SubscriptionNotActive = 5
}

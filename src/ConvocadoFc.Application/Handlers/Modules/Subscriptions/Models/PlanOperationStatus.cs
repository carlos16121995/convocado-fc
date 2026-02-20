namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

public enum PlanOperationStatus
{
    Success = 0,
    NotFound = 1,
    CodeAlreadyExists = 2,
    NameAlreadyExists = 3,
    InvalidCapacity = 4
}

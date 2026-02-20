using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ConvocadoFc.Application.Handlers.Modules.Subscriptions.Models;

namespace ConvocadoFc.Application.Handlers.Modules.Subscriptions.Interfaces;

public interface IPlanManagementHandler
{
    Task<IReadOnlyCollection<PlanDto>> ListPlansAsync(CancellationToken cancellationToken);
    Task<PlanDto?> GetPlanAsync(Guid planId, CancellationToken cancellationToken);
    Task<PlanOperationResult> CreatePlanAsync(CreatePlanCommand command, CancellationToken cancellationToken);
    Task<PlanOperationResult> UpdatePlanAsync(UpdatePlanCommand command, CancellationToken cancellationToken);
    Task<PlanOperationResult> DeactivatePlanAsync(Guid planId, CancellationToken cancellationToken);
}

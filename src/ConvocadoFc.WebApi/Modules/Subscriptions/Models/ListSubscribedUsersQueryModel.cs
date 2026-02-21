using ConvocadoFc.Domain.Models.Modules.Subscriptions;
using ConvocadoFc.WebApi.Models;

namespace ConvocadoFc.WebApi.Modules.Subscriptions.Models;

/// <summary>
/// Filtros para listagem de usuários com assinatura.
/// </summary>
public sealed record ListSubscribedUsersQueryModel : PaginationQueryModel
{
    /// <summary>
    /// Filtra pelo status da assinatura.
    /// </summary>
    public ESubscriptionStatus? Status { get; init; }
}

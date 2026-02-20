using System.Threading;
using System.Threading.Tasks;

namespace ConvocadoFc.Application.Handlers.Modules.Notifications.Interfaces;

public interface IMessageTransport<in TMessage> where TMessage : class
{
    Task DeliverAsync(TMessage message, CancellationToken cancellationToken = default);
}

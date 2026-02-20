namespace ConvocadoFc.Application.Abstractions.Notifications.Interfaces;

public interface IMessageTransport<in TMessage> where TMessage : class
{
    Task DeliverAsync(TMessage message, CancellationToken cancellationToken = default);
}

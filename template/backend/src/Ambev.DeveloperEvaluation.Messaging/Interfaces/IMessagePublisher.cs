namespace Ambev.DeveloperEvaluation.Messaging.Interfaces
{
    /// <summary>
    /// Publishes messages/events through the message bus.
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Publishes a message to the configured message bus.
        /// </summary>
        Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class;
    }
}

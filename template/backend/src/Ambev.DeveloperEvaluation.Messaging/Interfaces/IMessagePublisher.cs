namespace Ambev.DeveloperEvaluation.Messaging.Interfaces
{

    /// <summary>
    /// Defines the contract for publishing messages or events through a message bus.
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Publishes a message to the configured message bus.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message to publish.</typeparam>
        /// <param name="message">The message instance to be published.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default);
    }
}

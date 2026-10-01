namespace Ambev.DeveloperEvaluation.Messaging.Interfaces
{
    /// <summary>
    /// Defines the contract for subscribing to specific message types from the message bus.
    /// </summary>
    public interface IMessageSubscriber
    {
        /// <summary>
        /// Subscribes to messages of a given type and processes them using the registered handler.
        /// </summary>
        /// <typeparam name="TMessage">The message type to subscribe to.</typeparam>
        Task SubscribeAsync<TMessage>() where TMessage : class;
    }
}

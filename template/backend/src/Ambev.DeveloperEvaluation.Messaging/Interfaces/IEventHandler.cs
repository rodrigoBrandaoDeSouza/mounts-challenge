namespace Ambev.DeveloperEvaluation.Messaging.Interfaces
{
    /// <summary>
    /// Base interface for handling domain or integration events.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to handle.</typeparam>
    public interface IEventHandler<in TEvent>
    {
        /// <summary>
        /// Handles the incoming event asynchronously.
        /// </summary>
        /// <param name="event">The event instance to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
    }
}

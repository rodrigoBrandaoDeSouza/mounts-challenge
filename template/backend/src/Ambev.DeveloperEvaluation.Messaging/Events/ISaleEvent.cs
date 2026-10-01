namespace Ambev.DeveloperEvaluation.Messaging.Events
{
    /// <summary>
    /// Base interface for all sale-related events.
    /// </summary>
    public interface ISaleEvent
    {
        /// <summary>
        /// Gets or sets the unique identifier of the sale related to the event.
        /// </summary>
        Guid SaleId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the event occurred.
        /// </summary>
        DateTime OccurredAt { get; set; }
    }
}

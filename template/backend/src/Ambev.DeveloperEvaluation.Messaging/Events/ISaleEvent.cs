namespace Ambev.DeveloperEvaluation.Messaging.Events
{
    /// <summary>
    /// Base contract of every sale-related event.
    /// </summary>
    public interface ISaleEvent
    {
        /// <summary>Identifier of the sale related to the event.</summary>
        Guid SaleId { get; set; }

        /// <summary>When the event occurred (UTC).</summary>
        DateTime OccurredAt { get; set; }
    }
}

namespace Ambev.DeveloperEvaluation.Messaging.Events
{
    /// <summary>
    /// Event that represents the deletion or cancellation of a sale.
    /// </summary>
    public class SaleDeletedEvent : ISaleEvent
    {
        public Guid SaleId { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public bool Cancelled { get; set; } = true;
    }
}

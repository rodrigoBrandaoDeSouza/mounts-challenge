namespace Ambev.DeveloperEvaluation.Messaging.Events
{
    /// <summary>
    /// Published when a sale is deleted.
    /// </summary>
    public class SaleDeletedEvent : ISaleEvent
    {
        public Guid SaleId { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}

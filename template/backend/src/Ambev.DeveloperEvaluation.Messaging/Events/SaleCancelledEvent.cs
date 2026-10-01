namespace Ambev.DeveloperEvaluation.Messaging.Events
{
    /// <summary>
    /// Published when a sale is cancelled.
    /// </summary>
    public class SaleCancelledEvent : ISaleEvent
    {
        public Guid SaleId { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string SaleNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}

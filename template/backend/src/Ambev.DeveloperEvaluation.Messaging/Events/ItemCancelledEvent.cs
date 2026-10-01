namespace Ambev.DeveloperEvaluation.Messaging.Events
{
    /// <summary>
    /// Published when an item of a sale is cancelled.
    /// </summary>
    public class ItemCancelledEvent : ISaleEvent
    {
        public Guid SaleId { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string SaleNumber { get; set; } = string.Empty;
        public Guid ItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }

        /// <summary>New total amount of the sale, after the item cancellation.</summary>
        public decimal SaleTotalAmount { get; set; }
    }
}

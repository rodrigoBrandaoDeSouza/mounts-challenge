namespace Ambev.DeveloperEvaluation.Messaging.Events
{
    /// <summary>
    /// Event that represents the creation of a new sale.
    /// </summary>
    public class SaleCreatedEvent : ISaleEvent
    {
        public Guid SaleId { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        public string SaleNumber { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public List<SaleCreatedItem> Items { get; set; } = new();
    }

    public class SaleCreatedItem
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}

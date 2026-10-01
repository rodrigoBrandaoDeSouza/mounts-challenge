namespace Ambev.DeveloperEvaluation.Messaging.Events
{
    /// <summary>
    /// Published when a sale is modified.
    /// </summary>
    public class SaleModifiedEvent : ISaleEvent
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
        public List<SaleEventItem> Items { get; set; } = new();
    }
}

namespace Ambev.DeveloperEvaluation.Messaging.Events
{
    /// <summary>
    /// Item of a sale as carried by the sale events.
    /// </summary>
    public class SaleEventItem
    {
        public Guid ItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TotalPrice { get; set; }
        public bool Cancelled { get; set; }
    }
}

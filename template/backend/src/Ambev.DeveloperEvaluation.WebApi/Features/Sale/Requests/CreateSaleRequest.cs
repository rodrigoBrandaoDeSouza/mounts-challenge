namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.Requests
{
    /// <summary>
    /// Represents the HTTP request payload to create a new sale.
    /// </summary>
    /// <remarks>
    /// Customer, Branch and Product follow the External Identities pattern:
    /// the identifier from the owning context plus a denormalized description.
    /// </remarks>
    public class CreateSaleRequest
    {
        public string SaleNumber { get; set; } = string.Empty;

        /// <summary>Date of the sale. When omitted, the current UTC date/time is used.</summary>
        public DateTime? Date { get; set; }

        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public bool Cancelled { get; set; }
        public List<SaleItemRequest> Items { get; set; } = new();
    }

    /// <summary>
    /// Represents a sale item in the create/update requests.
    /// </summary>
    public class SaleItemRequest
    {
        /// <summary>Only used on update: identifier of an existing item. Omit it to add a new item.</summary>
        public Guid? Id { get; set; }

        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
        public bool Cancelled { get; set; }
    }
}

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.Requests
{
    /// <summary>
    /// Represents the HTTP request payload to update an existing sale.
    /// </summary>
    /// <remarks>
    /// Items are synchronized: items with <c>id</c> are updated, items without <c>id</c> are added
    /// and existing items not present in the payload are removed.
    /// </remarks>
    public class UpdateSaleRequest
    {
        public string SaleNumber { get; set; } = string.Empty;

        /// <summary>Date of the sale. When omitted, the original date is kept.</summary>
        public DateTime? Date { get; set; }

        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public List<SaleItemRequest> Items { get; set; } = new();
    }
}

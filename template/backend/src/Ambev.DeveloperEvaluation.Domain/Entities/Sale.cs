using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities
{
    /// <summary>
    /// Sale aggregate root.
    /// </summary>
    /// <remarks>
    /// Customer and Branch belong to other bounded contexts, so they are referenced using the
    /// External Identities pattern: only their identifiers are stored, together with a
    /// denormalized description (name) captured at the moment of the sale.
    /// </remarks>
    public class Sale : BaseEntity
    {
        public string SaleNumber { get; set; } = string.Empty;

        /// <summary>Date when the sale was made (UTC).</summary>
        public DateTime Date { get; set; }

        /// <summary>External identity of the customer (Customers context).</summary>
        public Guid CustomerId { get; set; }

        /// <summary>Denormalized customer name.</summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>External identity of the branch (Branches context).</summary>
        public Guid BranchId { get; set; }

        /// <summary>Denormalized branch name.</summary>
        public string BranchName { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }
        public bool Cancelled { get; set; }

        public virtual List<SaleItem> Items { get; set; } = new();
    }
}

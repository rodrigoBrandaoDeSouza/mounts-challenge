using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities
{
    public class SaleItem : BaseEntity
    {
        public Guid SaleId { get; set; }

        /// <summary>External identity of the product (Products context).</summary>
        public Guid ProductId { get; set; }

        /// <summary>Denormalized product name.</summary>
        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TotalPrice { get; set; }
        public bool Cancelled { get; set; }

        public virtual Sale Sale { get; set; } = null!;
    }
}

using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities
{
    /// <summary>
    /// Item of a <see cref="Sale"/>.
    /// </summary>
    /// <remarks>
    /// The product belongs to another bounded context, so it is referenced through the External
    /// Identities pattern: its identifier plus the denormalized name at the moment of the sale.
    /// </remarks>
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

        /// <summary>Total amount of the item (quantity x unit price - discount).</summary>
        public decimal TotalPrice { get; set; }

        public bool Cancelled { get; set; }

        public virtual Sale Sale { get; set; } = null!;

        /// <summary>Gross amount of the item, before the discount.</summary>
        public decimal GrossAmount => Quantity * UnitPrice;

        /// <summary>Discount amount applied to the item.</summary>
        public decimal DiscountAmount => GrossAmount - TotalPrice;

        /// <summary>
        /// Applies the discount percentage and recalculates the total of the item.
        /// </summary>
        public void ApplyDiscount(decimal discountPercent)
        {
            DiscountPercent = discountPercent;
            TotalPrice = Math.Round(GrossAmount * (100m - discountPercent) / 100m, 2, MidpointRounding.AwayFromZero);
        }
    }
}

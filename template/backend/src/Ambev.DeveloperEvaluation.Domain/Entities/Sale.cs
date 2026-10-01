using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Policies;

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

        /// <summary>Total amount of the sale (sum of the active items).</summary>
        public decimal TotalAmount { get; set; }

        public bool Cancelled { get; set; }

        public virtual List<SaleItem> Items { get; set; } = new();

        /// <summary>
        /// Applies the discount rules and recalculates the totals of the sale.
        /// </summary>
        public void RecalculateTotals() => SaleDiscountPolicy.Apply(this);

        /// <summary>
        /// Ensures the sale can still be changed.
        /// </summary>
        /// <exception cref="DomainException">When the sale is cancelled.</exception>
        public void EnsureNotCancelled()
        {
            if (Cancelled)
                throw new DomainException($"The sale {SaleNumber} is cancelled and cannot be modified.");
        }

        /// <summary>
        /// Cancels the sale.
        /// </summary>
        public void Cancel()
        {
            if (Cancelled)
                throw new DomainException($"The sale {SaleNumber} is already cancelled.");

            Cancelled = true;
        }

        /// <summary>
        /// Cancels one item of the sale and recalculates the discounts and totals.
        /// </summary>
        /// <returns>The cancelled item.</returns>
        public SaleItem CancelItem(Guid itemId)
        {
            EnsureNotCancelled();

            var item = Items.FirstOrDefault(i => i.Id == itemId)
                ?? throw new ResourceNotFoundException(
                    "Sale item not found",
                    $"The item with ID {itemId} does not exist in the sale {Id}");

            if (item.Cancelled)
                throw new DomainException($"The item {itemId} is already cancelled.");

            item.Cancelled = true;
            RecalculateTotals();
            return item;
        }

        /// <summary>
        /// Synchronizes the items of the sale with the informed ones: items with a known <c>Id</c> are
        /// updated, items without <c>Id</c> (or with an unknown one) are added and the current items that
        /// were not informed are removed. Discounts and totals are recalculated afterwards.
        /// </summary>
        public void ReplaceItems(IEnumerable<SaleItem> items)
        {
            EnsureNotCancelled();

            var incoming = items.ToList();
            var informedIds = incoming
                .Where(i => i.Id != Guid.Empty)
                .Select(i => i.Id)
                .ToHashSet();

            Items.RemoveAll(i => !informedIds.Contains(i.Id));

            foreach (var item in incoming)
            {
                var current = item.Id == Guid.Empty
                    ? null
                    : Items.FirstOrDefault(i => i.Id == item.Id);

                if (current is null)
                {
                    Items.Add(new SaleItem
                    {
                        SaleId = Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });
                }
                else
                {
                    current.ProductId = item.ProductId;
                    current.ProductName = item.ProductName;
                    current.Quantity = item.Quantity;
                    current.UnitPrice = item.UnitPrice;
                }
            }

            RecalculateTotals();
        }
    }
}

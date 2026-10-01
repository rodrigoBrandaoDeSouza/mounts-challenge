using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Services
{
    /// <summary>
    /// Orchestrates the sale operations: applies the business rules, persists the aggregate
    /// and publishes the domain events (SaleCreated, SaleModified, SaleCancelled, ItemCancelled, SaleDeleted).
    /// </summary>
    public interface ISaleService
    {
        /// <summary>Creates a sale, applying the discount rules. Publishes SaleCreated.</summary>
        Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

        /// <summary>Persists the changes of a sale, applying the discount rules. Publishes SaleModified.</summary>
        Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

        /// <summary>Cancels a sale. Publishes SaleCancelled.</summary>
        Task<Sale> CancelAsync(Guid saleId, CancellationToken cancellationToken = default);

        /// <summary>Cancels an item of a sale. Publishes ItemCancelled.</summary>
        Task<Sale> CancelItemAsync(Guid saleId, Guid itemId, CancellationToken cancellationToken = default);

        /// <summary>Deletes a sale. Publishes SaleDeleted. Returns false when the sale does not exist.</summary>
        Task<bool> DeleteAsync(Guid saleId, CancellationToken cancellationToken = default);

        /// <summary>Retrieves a sale by its identifier, or null when it does not exist.</summary>
        Task<Sale?> GetByIdAsync(Guid saleId, CancellationToken cancellationToken = default);

        /// <summary>Retrieves a page of sales.</summary>
        Task<PagedResult<Sale>> ListAsync(QueryOptions options, CancellationToken cancellationToken = default);
    }
}

using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories
{
    /// <summary>
    /// Repository of the <see cref="Sale"/> aggregate.
    /// </summary>
    public interface ISaleRepository
    {
        /// <summary>Persists a new sale.</summary>
        Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

        /// <summary>Retrieves a sale (with its items) by its identifier, or null when it does not exist.</summary>
        Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Retrieves a page of sales applying the filters and the ordering.</summary>
        Task<PagedResult<Sale>> ListAsync(QueryOptions options, CancellationToken cancellationToken = default);

        /// <summary>Persists the changes of an existing sale (including added, changed and removed items).</summary>
        Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

        /// <summary>Deletes a sale. Returns false when it does not exist.</summary>
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Checks whether another sale already uses the given sale number.</summary>
        Task<bool> SaleNumberExistsAsync(string saleNumber, Guid? ignoreSaleId = null, CancellationToken cancellationToken = default);
    }
}

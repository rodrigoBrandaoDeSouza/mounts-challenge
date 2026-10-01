using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Services
{
    /// <summary>
    /// Defines the application-level service responsible for orchestrating sale operations.
    /// This service operates on domain entities and integrates with the messaging layer
    /// to trigger asynchronous workflows using Rebus.
    /// </summary>
    public interface ISaleService
    {
        /// <summary>
        /// Creates a new sale asynchronously by publishing a domain event to the messaging system.
        /// </summary>
        /// <param name="sale">The sale entity to create.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created sale entity.</returns>
        Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing sale asynchronously by publishing a corresponding event.
        /// </summary>
        /// <param name="sale">The sale entity with updated information.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated sale entity.</returns>
        Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a sale asynchronously by publishing a deletion event to the messaging system.
        /// </summary>
        /// <param name="saleId">The unique identifier of the sale to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the delete request was accepted; false otherwise.</returns>
        Task<bool> DeleteAsync(Guid saleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a sale by its unique identifier.
        /// </summary>
        /// <param name="saleId">The unique identifier of the sale to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The sale entity if found; null otherwise.</returns>
        Task<Sale?> GetByIdAsync(Guid saleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Fetches all sales with optional filtering or pagination parameters.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A collection of sales.</returns>
        Task<IEnumerable<Sale>> FetchAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    }
}

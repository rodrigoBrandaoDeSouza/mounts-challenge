using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Repository of <see cref="Cart"/>.
/// </summary>
public interface ICartRepository
{
    Task<Cart> CreateAsync(Cart cart, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a cart (with its products) by its identifier, or null when it does not exist.</summary>
    Task<Cart?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedResult<Cart>> ListAsync(QueryOptions options, CancellationToken cancellationToken = default);
    Task<Cart> UpdateAsync(Cart cart, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

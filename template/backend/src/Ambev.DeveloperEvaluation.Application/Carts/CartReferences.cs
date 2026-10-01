using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Carts;

internal static class CartReferences
{
    /// <summary>
    /// Ensures that the user and every product referenced by the cart exist.
    /// </summary>
    public static async Task EnsureExistAsync(
        IUserRepository userRepository,
        IProductRepository productRepository,
        CartInput cart,
        CancellationToken cancellationToken)
    {
        if (!await userRepository.ExistsAsync(cart.UserId, cancellationToken))
            throw ResourceNotFoundException.For("User", cart.UserId);

        var productIds = cart.Products.Select(p => p.ProductId).Distinct().ToList();
        var existing = await productRepository.GetExistingIdsAsync(productIds, cancellationToken);
        var missing = productIds.Except(existing).ToList();

        if (missing.Count > 0)
            throw new ResourceNotFoundException(
                "Product not found",
                $"The products with ID {string.Join(", ", missing)} do not exist in our database");
    }
}

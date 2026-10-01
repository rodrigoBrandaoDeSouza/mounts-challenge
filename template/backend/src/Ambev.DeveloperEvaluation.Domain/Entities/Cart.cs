namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Shopping cart of a user.
/// </summary>
public class Cart
{
    public int Id { get; set; }

    /// <summary>Identifier of the user that owns the cart.</summary>
    public int UserId { get; set; }

    public DateTime Date { get; set; }

    public virtual List<CartProduct> Products { get; set; } = new();

    /// <summary>
    /// Replaces the products of the cart.
    /// </summary>
    public void ReplaceProducts(IEnumerable<CartProduct> products)
    {
        Products.Clear();
        foreach (var product in products)
        {
            Products.Add(new CartProduct
            {
                CartId = Id,
                ProductId = product.ProductId,
                Quantity = product.Quantity
            });
        }
    }
}

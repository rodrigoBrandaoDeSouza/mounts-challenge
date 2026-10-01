namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Product (and quantity) inside a <see cref="Cart"/>.
/// </summary>
public class CartProduct
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }

    public virtual Cart Cart { get; set; } = null!;
}

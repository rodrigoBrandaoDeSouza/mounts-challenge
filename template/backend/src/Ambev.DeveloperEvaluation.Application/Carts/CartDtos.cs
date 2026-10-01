namespace Ambev.DeveloperEvaluation.Application.Carts;

/// <summary>
/// Cart returned by the API.
/// </summary>
public class CartResult
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CartProductDto> Products { get; set; } = new();
}

/// <summary>
/// Product (and quantity) inside a cart.
/// </summary>
public class CartProductDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Data of a cart informed on create/update.
/// </summary>
public abstract class CartInput
{
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public List<CartProductDto> Products { get; set; } = new();
}

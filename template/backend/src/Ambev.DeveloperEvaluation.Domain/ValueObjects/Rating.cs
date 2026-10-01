namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// Rating of a product (value object owned by <see cref="Entities.Product"/>).
/// </summary>
public class Rating
{
    public decimal Rate { get; set; }
    public int Count { get; set; }
}

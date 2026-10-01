namespace Ambev.DeveloperEvaluation.Application.Products;

/// <summary>
/// Data of a product informed on create/update.
/// </summary>
public abstract class ProductInput
{
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public ProductRatingDto Rating { get; set; } = new();
}

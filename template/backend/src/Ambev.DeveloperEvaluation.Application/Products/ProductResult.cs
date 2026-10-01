namespace Ambev.DeveloperEvaluation.Application.Products;

/// <summary>
/// Product returned by the API.
/// </summary>
public class ProductResult
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public ProductRatingDto Rating { get; set; } = new();
}

/// <summary>
/// Rating of a product.
/// </summary>
public class ProductRatingDto
{
    public decimal Rate { get; set; }
    public int Count { get; set; }
}

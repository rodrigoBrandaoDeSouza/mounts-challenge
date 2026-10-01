namespace Ambev.DeveloperEvaluation.Application.Sales;

/// <summary>
/// Item informed when creating or updating a sale. Totals and discounts are always calculated by the API.
/// </summary>
public class SaleItemInput
{
    /// <summary>
    /// Only used on update: identifier of an existing item of the sale. Omit it to add a new item.
    /// Existing items that are not informed are removed.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>External identity of the product (Products context).</summary>
    public Guid ProductId { get; set; }

    /// <summary>Denormalized product name.</summary>
    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

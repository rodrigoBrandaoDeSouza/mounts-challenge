using MediatR;
using System.Text.Json.Serialization;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Updates a sale. Items with <c>id</c> are updated, items without <c>id</c> are added and
/// existing items not informed are removed. Cancelled sales cannot be updated.
/// </summary>
public class UpdateSaleCommand : IRequest<SaleResult>
{
    /// <summary>Identifier of the sale (taken from the route).</summary>
    [JsonIgnore]
    public Guid Id { get; set; }

    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>Date of the sale. When omitted, the original date is kept.</summary>
    public DateTime? Date { get; set; }

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public List<SaleItemInput> Items { get; set; } = new();
}

using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Creates a sale. Customer, Branch and Product follow the External Identities pattern:
/// the identifier from the owning context plus a denormalized description.
/// </summary>
public class CreateSaleCommand : IRequest<SaleResult>
{
    public string SaleNumber { get; set; } = string.Empty;

    /// <summary>Date of the sale. When omitted, the current UTC date/time is used.</summary>
    public DateTime? Date { get; set; }

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public List<SaleItemInput> Items { get; set; } = new();
}

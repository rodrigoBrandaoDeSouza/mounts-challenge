using Ambev.DeveloperEvaluation.Application.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales;

/// <summary>
/// Fields of the sale resource that can be used in <c>_order</c> and filters.
/// </summary>
public static class SaleQueryFields
{
    public static readonly QueryFieldMap<Sale> Map = new(
        new Dictionary<string, string>
        {
            ["id"] = nameof(Sale.Id),
            ["saleNumber"] = nameof(Sale.SaleNumber),
            ["date"] = nameof(Sale.Date),
            ["customerId"] = nameof(Sale.CustomerId),
            ["customerName"] = nameof(Sale.CustomerName),
            ["branchId"] = nameof(Sale.BranchId),
            ["branchName"] = nameof(Sale.BranchName),
            ["totalAmount"] = nameof(Sale.TotalAmount),
            ["cancelled"] = nameof(Sale.Cancelled)
        },
        new OrderCriterion(nameof(Sale.Date), Descending: true),
        new OrderCriterion(nameof(Sale.SaleNumber), Descending: false));
}

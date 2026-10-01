using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>
/// Retrieves a sale by its identifier.
/// </summary>
public record GetSaleQuery(Guid Id) : IRequest<SaleResult>;

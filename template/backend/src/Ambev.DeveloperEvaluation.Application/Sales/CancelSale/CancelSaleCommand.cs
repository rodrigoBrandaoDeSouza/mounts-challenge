using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

/// <summary>
/// Cancels a sale (publishes SaleCancelled).
/// </summary>
public record CancelSaleCommand(Guid Id) : IRequest<SaleResult>;

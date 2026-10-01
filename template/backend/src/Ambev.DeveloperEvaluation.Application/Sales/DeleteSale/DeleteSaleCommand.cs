using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

/// <summary>
/// Deletes a sale.
/// </summary>
public record DeleteSaleCommand(Guid Id) : IRequest<Unit>;

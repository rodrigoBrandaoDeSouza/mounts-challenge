using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

/// <summary>
/// Cancels an item of a sale (publishes ItemCancelled). Discounts and totals are recalculated.
/// </summary>
public record CancelSaleItemCommand(Guid SaleId, Guid ItemId) : IRequest<SaleResult>;

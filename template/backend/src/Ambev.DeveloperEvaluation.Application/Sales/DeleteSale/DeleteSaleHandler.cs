using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Services;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

public class DeleteSaleHandler : IRequestHandler<DeleteSaleCommand, Unit>
{
    private readonly ISaleService _saleService;

    public DeleteSaleHandler(ISaleService saleService)
    {
        _saleService = saleService;
    }

    public async Task<Unit> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _saleService.DeleteAsync(request.Id, cancellationToken);
        if (!deleted)
            throw ResourceNotFoundException.For("Sale", request.Id);

        return Unit.Value;
    }
}

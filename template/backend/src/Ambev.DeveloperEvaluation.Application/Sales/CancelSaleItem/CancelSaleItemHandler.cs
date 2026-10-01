using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, SaleResult>
{
    private readonly ISaleService _saleService;
    private readonly IMapper _mapper;

    public CancelSaleItemHandler(ISaleService saleService, IMapper mapper)
    {
        _saleService = saleService;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleService.CancelItemAsync(request.SaleId, request.ItemId, cancellationToken);
        return _mapper.Map<SaleResult>(sale);
    }
}

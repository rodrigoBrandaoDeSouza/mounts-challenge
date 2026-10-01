using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public class GetSaleHandler : IRequestHandler<GetSaleQuery, SaleResult>
{
    private readonly ISaleService _saleService;
    private readonly IMapper _mapper;

    public GetSaleHandler(ISaleService saleService, IMapper mapper)
    {
        _saleService = saleService;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(GetSaleQuery request, CancellationToken cancellationToken)
    {
        var sale = await _saleService.GetByIdAsync(request.Id, cancellationToken)
            ?? throw ResourceNotFoundException.For("Sale", request.Id);

        return _mapper.Map<SaleResult>(sale);
    }
}

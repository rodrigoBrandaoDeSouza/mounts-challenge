using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, SaleResult>
{
    private readonly ISaleService _saleService;
    private readonly IMapper _mapper;

    public CreateSaleHandler(ISaleService saleService, IMapper mapper)
    {
        _saleService = saleService;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = _mapper.Map<Sale>(request);
        sale.Date = (request.Date ?? DateTime.UtcNow).ToUtc();

        var created = await _saleService.CreateAsync(sale, cancellationToken);
        return _mapper.Map<SaleResult>(created);
    }
}

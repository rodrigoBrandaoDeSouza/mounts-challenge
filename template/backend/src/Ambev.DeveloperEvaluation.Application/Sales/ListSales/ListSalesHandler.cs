using Ambev.DeveloperEvaluation.Application.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesHandler : IRequestHandler<ListSalesQuery, PagedResult<SaleResult>>
{
    private readonly ISaleService _saleService;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleService saleService, IMapper mapper)
    {
        _saleService = saleService;
        _mapper = mapper;
    }

    public async Task<PagedResult<SaleResult>> Handle(ListSalesQuery request, CancellationToken cancellationToken)
    {
        var options = QueryOptionsParser.Parse(request, SaleQueryFields.Map);
        var sales = await _saleService.ListAsync(options, cancellationToken);
        return sales.Map(s => _mapper.Map<SaleResult>(s));
    }
}

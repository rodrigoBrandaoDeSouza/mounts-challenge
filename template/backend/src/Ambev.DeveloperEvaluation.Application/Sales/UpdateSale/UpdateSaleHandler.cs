using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, SaleResult>
{
    private readonly ISaleService _saleService;
    private readonly IMapper _mapper;

    public UpdateSaleHandler(ISaleService saleService, IMapper mapper)
    {
        _saleService = saleService;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleService.GetByIdAsync(request.Id, cancellationToken)
            ?? throw ResourceNotFoundException.For("Sale", request.Id);

        sale.EnsureNotCancelled();

        sale.SaleNumber = request.SaleNumber;
        sale.CustomerId = request.CustomerId;
        sale.CustomerName = request.CustomerName;
        sale.BranchId = request.BranchId;
        sale.BranchName = request.BranchName;

        // Keep the original sale date unless a new one is informed.
        if (request.Date.HasValue)
            sale.Date = request.Date.Value.ToUtc();

        sale.ReplaceItems(_mapper.Map<List<SaleItem>>(request.Items));

        var updated = await _saleService.UpdateAsync(sale, cancellationToken);
        return _mapper.Map<SaleResult>(updated);
    }
}

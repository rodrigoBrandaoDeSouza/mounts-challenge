using Ambev.DeveloperEvaluation.Application.Models;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sale.FetchSales
{
    /// <summary>
    /// Handler for processing <see cref="FetchSalesCommand"/> requests.
    /// </summary>
    /// <remarks>
    /// This handler retrieves all sales from the repository,
    /// maps them into response objects, and returns the collection.
    /// </remarks>
    public class FetchSalesHandler : IRequestHandler<FetchSalesCommand, OperationResult<List<Domain.Entities.Sale>>>
    {
        private readonly ISaleService _saleService;
        private readonly IMapper _mapper;

        public FetchSalesHandler(ISaleService saleService, IMapper mapper)
        {
            _saleService = saleService;
            _mapper = mapper;
        }

        public async Task<OperationResult<List<Domain.Entities.Sale>>> Handle(FetchSalesCommand request, CancellationToken cancellationToken)
        {
            var sales = await _saleService.FetchAsync(request.Page, request.PageSize, cancellationToken);

            if (sales is not null && sales.Any())
                return OperationResult<List<Domain.Entities.Sale>>.Ok(sales.ToList(), "Sales found");
            else
                return OperationResult<List<Domain.Entities.Sale>>.Fail("No Sales found");

        }
    }
}

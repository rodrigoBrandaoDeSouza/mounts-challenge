using Ambev.DeveloperEvaluation.Application.Models;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sale.GetSale
{
    public class GetSaleByIdHandler : IRequestHandler<GetSaleByIdCommand, OperationResult<Domain.Entities.Sale>>
    {
        private readonly ISaleService _saleService;
        private readonly IMapper _mapper;

        public GetSaleByIdHandler(ISaleService saleService, IMapper mapper)
        {
            _saleService = saleService;
            _mapper = mapper;
        }

        /// <summary>
        /// Handler for processing <see cref="UpdateSaleCommand"/> requests.
        /// </summary>
        /// <remarks>
        /// This handler validates the request, retrieves the sale from the repository,
        /// applies updates, calculates discounts, and persists the changes.
        /// </remarks>
        public async Task<OperationResult<Domain.Entities.Sale>> Handle(GetSaleByIdCommand request, CancellationToken cancellationToken)
        {
            var sale = await _saleService.GetByIdAsync(request.Id);

            if (sale is not null)
                return OperationResult<Domain.Entities.Sale>.Ok(sale, "Sale found");
            else
                return OperationResult<Domain.Entities.Sale>.Fail("Sale not found");

        }
    }
}

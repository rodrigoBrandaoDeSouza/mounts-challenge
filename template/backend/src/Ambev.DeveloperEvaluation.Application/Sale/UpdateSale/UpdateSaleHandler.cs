using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Models;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sale.UpdateSale
{
    public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, OperationResult<Domain.Entities.Sale>>
    {
        private readonly ISaleService _saleService;
        private readonly IMapper _mapper;

        public UpdateSaleHandler(ISaleService saleService, IMapper mapper)
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
        public async Task<OperationResult<Domain.Entities.Sale>> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
        {
            var existingSale = await _saleService.GetByIdAsync(request.Id, cancellationToken);

            if(existingSale is null)
                return OperationResult<Domain.Entities.Sale>.Fail($"Sale with ID {request.Id} not found");

            var sale = _mapper.Map<Domain.Entities.Sale>(request);
            sale.Id = request.Id;

            // Keep the original sale date unless a new one is informed.
            sale.Date = request.Date.HasValue ? request.Date.Value.ToUtc() : existingSale.Date;

            // Cancellation is not changed through the update operation.
            sale.Cancelled = existingSale.Cancelled;

            var saleUpdated = await _saleService.UpdateAsync(sale, cancellationToken);

            return OperationResult<Domain.Entities.Sale>.Ok(saleUpdated, "Sale updated successfully");
        }
    }
}

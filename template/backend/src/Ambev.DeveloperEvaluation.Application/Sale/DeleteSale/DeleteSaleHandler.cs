using Ambev.DeveloperEvaluation.Application.Models;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sale.DeleteSale
{
    public class DeleteSaleHandler : IRequestHandler<DeleteSaleCommand, OperationResult<Domain.Entities.Sale>>
    {
        private readonly ISaleService _saleService;
        private readonly IMapper _mapper;

        public DeleteSaleHandler(ISaleService saleService, IMapper mapper)
        {
            _saleService = saleService;
            _mapper = mapper;
        }

        /// <summary>
        /// Handler for processing <see cref="DeleteSaleCommand"/> requests.
        /// </summary>
        /// <remarks>
        /// This handler is responsible for validating the delete request, 
        /// performing the deletion using <see cref="ISaleRepository"/>,
        /// and returning the result as <see cref="DeleteSaleResponse"/>.
        /// </remarks>
        public async Task<OperationResult<Domain.Entities.Sale>> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
        {
            var deleteSuccessfull = await _saleService.DeleteAsync(request.Id);
            
            if(deleteSuccessfull)
                return OperationResult<Domain.Entities.Sale>.Ok("Sale deleted.");
            else
                return OperationResult<Domain.Entities.Sale>.Fail("Fail to delete Sale.");

        }
    }
}

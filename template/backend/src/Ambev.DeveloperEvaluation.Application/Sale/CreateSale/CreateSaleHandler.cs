using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Application.Models;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sale.CreateSale
{
    public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, OperationResult<Domain.Entities.Sale>>
    {
        private readonly ISaleService _saleService;
        private readonly IMapper _mapper;

        public CreateSaleHandler(ISaleService saleService, IMapper mapper)
        {
            _saleService = saleService;
            _mapper = mapper;
        }

        public async Task<OperationResult<Domain.Entities.Sale>> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
        {
            var sale = _mapper.Map<Domain.Entities.Sale>(request);
            sale.Date = (request.Date ?? DateTime.UtcNow).ToUtc();

            var createdSale = await _saleService.CreateAsync(sale, cancellationToken);

            if(createdSale is not null)
                return OperationResult<Domain.Entities.Sale>.Ok(createdSale, "Sale created");
            else
                return OperationResult<Domain.Entities.Sale>.Fail("Fail to create sale");
        }
    }
}

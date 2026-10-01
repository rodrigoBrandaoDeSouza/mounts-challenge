using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sale.CreateSale
{
    /// <summary>
    /// AutoMapper profile for mapping between CreateSaleCommand and Sale entity.
    /// </summary>
    public class CreateSaleProfile : Profile
    {
        public CreateSaleProfile()
        {
            // Date is resolved by the handler (defaults to UtcNow when not informed).
            CreateMap<CreateSaleCommand, Domain.Entities.Sale>()
                .ForMember(dest => dest.Date, opt => opt.Ignore());

            CreateMap<CreateSaleItemDto, SaleItem>();
        }
    }
}

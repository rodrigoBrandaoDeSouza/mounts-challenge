using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales;

/// <summary>
/// AutoMapper profile of the sales use cases.
/// </summary>
public class SaleProfile : Profile
{
    public SaleProfile()
    {
        CreateMap<Sale, SaleResult>();
        CreateMap<SaleItem, SaleItemResult>();

        CreateMap<SaleItemInput, SaleItem>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id ?? Guid.Empty))
            .ForMember(d => d.SaleId, o => o.Ignore())
            .ForMember(d => d.Sale, o => o.Ignore())
            .ForMember(d => d.DiscountPercent, o => o.Ignore())
            .ForMember(d => d.TotalPrice, o => o.Ignore())
            .ForMember(d => d.Cancelled, o => o.Ignore());

        // Date is resolved by the handler; totals, discounts and cancellation are calculated by the domain.
        CreateMap<CreateSaleCommand, Sale>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Date, o => o.Ignore())
            .ForMember(d => d.TotalAmount, o => o.Ignore())
            .ForMember(d => d.Cancelled, o => o.Ignore());
    }
}

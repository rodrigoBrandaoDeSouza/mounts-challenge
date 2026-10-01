using Ambev.DeveloperEvaluation.Application.Sale.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sale.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sale.FetchSales;
using Ambev.DeveloperEvaluation.Application.Sale.GetSale;
using Ambev.DeveloperEvaluation.Application.Sale.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.Requests;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Mappings
{
    /// <summary>
    /// Defines AutoMapper mappings between WebApi requests and Application commands.
    /// </summary>
    public class SaleApiProfile : Profile
    {
        public SaleApiProfile()
        {
            CreateMap<CreateSaleRequest, CreateSaleCommand>();
            CreateMap<SaleItemRequest, CreateSaleItemDto>();

            CreateMap<UpdateSaleRequest, UpdateSaleCommand>();
            CreateMap<SaleItemRequest, UpdateSaleItemDto>();

            CreateMap<DeleteSaleRequest, DeleteSaleCommand>();

            CreateMap<GetSaleByIdRequest, GetSaleByIdCommand>();

            CreateMap<FetchSalesRequest, FetchSalesCommand>();
        }
    }
}

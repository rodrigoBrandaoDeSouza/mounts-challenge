using Ambev.DeveloperEvaluation.Application.Carts.CreateCart;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Carts;

public class CartProfile : Profile
{
    public CartProfile()
    {
        CreateMap<Cart, CartResult>();
        CreateMap<CartProduct, CartProductDto>();

        CreateMap<CartProductDto, CartProduct>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CartId, o => o.Ignore())
            .ForMember(d => d.Cart, o => o.Ignore());

        CreateMap<CreateCartCommand, Cart>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Date, o => o.Ignore());
    }
}

using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Products;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Rating, ProductRatingDto>().ReverseMap();
        CreateMap<Product, ProductResult>();

        CreateMap<CreateProductCommand, Product>()
            .ForMember(d => d.Id, o => o.Ignore());

        CreateMap<UpdateProductCommand, Product>();
    }
}

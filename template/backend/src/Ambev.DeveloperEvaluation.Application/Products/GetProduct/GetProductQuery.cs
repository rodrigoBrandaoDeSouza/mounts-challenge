using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.GetProduct;

public record GetProductQuery(int Id) : IRequest<ProductResult>;

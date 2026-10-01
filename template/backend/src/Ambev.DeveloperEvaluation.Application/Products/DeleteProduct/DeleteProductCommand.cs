using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.DeleteProduct;

public record DeleteProductCommand(int Id) : IRequest<Unit>;

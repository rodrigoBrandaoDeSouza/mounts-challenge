using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.GetCart;

public record GetCartQuery(int Id) : IRequest<CartResult>;

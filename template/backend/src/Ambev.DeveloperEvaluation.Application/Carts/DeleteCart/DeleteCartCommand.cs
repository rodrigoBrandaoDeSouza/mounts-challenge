using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.DeleteCart;

public record DeleteCartCommand(int Id) : IRequest<Unit>;

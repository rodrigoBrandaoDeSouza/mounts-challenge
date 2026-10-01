using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.DeleteCart;

public class DeleteCartHandler : IRequestHandler<DeleteCartCommand, Unit>
{
    private readonly ICartRepository _repository;

    public DeleteCartHandler(ICartRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(DeleteCartCommand request, CancellationToken cancellationToken)
    {
        if (!await _repository.DeleteAsync(request.Id, cancellationToken))
            throw ResourceNotFoundException.For("Cart", request.Id);

        return Unit.Value;
    }
}

using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.GetCart;

public class GetCartHandler : IRequestHandler<GetCartQuery, CartResult>
{
    private readonly ICartRepository _repository;
    private readonly IMapper _mapper;

    public GetCartHandler(ICartRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CartResult> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw ResourceNotFoundException.For("Cart", request.Id);

        return _mapper.Map<CartResult>(cart);
    }
}

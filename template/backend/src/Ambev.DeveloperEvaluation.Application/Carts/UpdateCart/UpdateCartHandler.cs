using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;

public class UpdateCartHandler : IRequestHandler<UpdateCartCommand, CartResult>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public UpdateCartHandler(ICartRepository cartRepository, IUserRepository userRepository, IProductRepository productRepository, IMapper mapper)
    {
        _cartRepository = cartRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<CartResult> Handle(UpdateCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw ResourceNotFoundException.For("Cart", request.Id);

        await CartReferences.EnsureExistAsync(_userRepository, _productRepository, request, cancellationToken);

        cart.UserId = request.UserId;
        cart.Date = request.Date.ToUtc();
        cart.ReplaceProducts(_mapper.Map<List<CartProduct>>(request.Products));

        var updated = await _cartRepository.UpdateAsync(cart, cancellationToken);
        return _mapper.Map<CartResult>(updated);
    }
}

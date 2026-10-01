using Ambev.DeveloperEvaluation.Application.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.CreateCart;

public class CreateCartHandler : IRequestHandler<CreateCartCommand, CartResult>
{
    private readonly ICartRepository _cartRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public CreateCartHandler(ICartRepository cartRepository, IUserRepository userRepository, IProductRepository productRepository, IMapper mapper)
    {
        _cartRepository = cartRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<CartResult> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        await CartReferences.EnsureExistAsync(_userRepository, _productRepository, request, cancellationToken);

        var cart = _mapper.Map<Cart>(request);
        cart.Date = request.Date.ToUtc();

        var created = await _cartRepository.CreateAsync(cart, cancellationToken);
        return _mapper.Map<CartResult>(created);
    }
}

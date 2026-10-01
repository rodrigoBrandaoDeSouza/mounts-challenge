using Ambev.DeveloperEvaluation.Application.Carts;
using Ambev.DeveloperEvaluation.Application.Carts.CreateCart;
using Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Carts;

public class CartHandlersTests
{
    private readonly ICartRepository _cartRepository = Substitute.For<ICartRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IMapper _mapper = TestMapper.Create();

    public CartHandlersTests()
    {
        _userRepository.ExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        _productRepository.GetExistingIdsAsync(Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>())
            .Returns(new List<int> { 1, 2 });
        _cartRepository.CreateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var cart = call.Arg<Cart>();
                cart.Id = 10;
                return cart;
            });
        _cartRepository.UpdateAsync(Arg.Any<Cart>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<Cart>());
    }

    private static CreateCartCommand Command(params int[] productIds) => new()
    {
        UserId = 1,
        Date = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc),
        Products = productIds.Select(id => new CartProductDto { ProductId = id, Quantity = 2 }).ToList()
    };

    private CreateCartHandler CreateHandler() => new(_cartRepository, _userRepository, _productRepository, _mapper);

    [Fact(DisplayName = "Create persists the cart and returns it")]
    public async Task Create_ReturnsCart()
    {
        var result = await CreateHandler().Handle(Command(1, 2), CancellationToken.None);

        result.Id.Should().Be(10);
        result.UserId.Should().Be(1);
        result.Products.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Create with an unknown user throws ResourceNotFoundException")]
    public async Task Create_UnknownUser_Throws()
    {
        var command = Command(1);
        command.UserId = 2;

        var act = () => CreateHandler().Handle(command, CancellationToken.None);

        (await act.Should().ThrowAsync<ResourceNotFoundException>()).Which.Error.Should().Be("User not found");
    }

    [Fact(DisplayName = "Create with unknown products throws ResourceNotFoundException")]
    public async Task Create_UnknownProducts_Throws()
    {
        var act = () => CreateHandler().Handle(Command(1, 3), CancellationToken.None);

        (await act.Should().ThrowAsync<ResourceNotFoundException>()).Which.Detail.Should().Contain("3");
    }

    [Fact(DisplayName = "Update replaces the products of the cart")]
    public async Task Update_ReplacesProducts()
    {
        var cart = new Cart
        {
            Id = 10,
            UserId = 1,
            Date = DateTime.UtcNow,
            Products = new List<CartProduct> { new() { Id = 1, CartId = 10, ProductId = 1, Quantity = 1 } }
        };
        _cartRepository.GetByIdAsync(10, Arg.Any<CancellationToken>()).Returns(cart);

        var command = new UpdateCartCommand
        {
            Id = 10,
            UserId = 1,
            Date = DateTime.UtcNow,
            Products = new List<CartProductDto> { new() { ProductId = 2, Quantity = 5 } }
        };

        var result = await new UpdateCartHandler(_cartRepository, _userRepository, _productRepository, _mapper).Handle(command, CancellationToken.None);

        result.Products.Should().ContainSingle().Which.Should().BeEquivalentTo(new CartProductDto { ProductId = 2, Quantity = 5 });
    }

    [Fact(DisplayName = "Cart validator rejects duplicated products and invalid quantities")]
    public void Validator_RejectsInvalidCart()
    {
        var command = Command(1, 1);
        command.Products[0].Quantity = 0;

        var result = new CreateCartValidator().Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}

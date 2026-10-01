using Ambev.DeveloperEvaluation.Application.Products;
using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Application.Products.DeleteProduct;
using Ambev.DeveloperEvaluation.Application.Products.GetProduct;
using Ambev.DeveloperEvaluation.Application.Products.ListCategories;
using Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Ambev.DeveloperEvaluation.Unit.TestData;
using AutoMapper;
using Bogus;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Products;

public class ProductHandlersTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly IMapper _mapper = TestMapper.Create();
    private readonly Faker _faker = new();

    private CreateProductCommand ValidCommand() => new()
    {
        Title = _faker.Commerce.ProductName(),
        Price = 109.95m,
        Description = _faker.Commerce.ProductDescription(),
        Category = "men's clothing",
        Image = "https://fakestoreapi.com/img/81fPKd-2AYL._AC_SL1500_.jpg",
        Rating = new ProductRatingDto { Rate = 3.9m, Count = 120 }
    };

    [Fact(DisplayName = "Create persists the product and returns it")]
    public async Task Create_ReturnsProduct()
    {
        var command = ValidCommand();
        _repository.CreateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var product = call.Arg<Product>();
                product.Id = 1;
                return product;
            });

        var result = await new CreateProductHandler(_repository, _mapper).Handle(command, CancellationToken.None);

        result.Id.Should().Be(1);
        result.Should().BeEquivalentTo(command, options => options.ExcludingMissingMembers());
    }

    [Fact(DisplayName = "Update changes the product")]
    public async Task Update_ChangesProduct()
    {
        var product = new Product { Id = 5, Title = "Old", Price = 1m, Category = "old", Rating = new Rating() };
        _repository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(product);
        _repository.UpdateAsync(product, Arg.Any<CancellationToken>()).Returns(product);

        var create = ValidCommand();
        var command = new UpdateProductCommand
        {
            Id = 5,
            Title = create.Title,
            Price = create.Price,
            Description = create.Description,
            Category = create.Category,
            Image = create.Image,
            Rating = create.Rating
        };

        var result = await new UpdateProductHandler(_repository, _mapper).Handle(command, CancellationToken.None);

        result.Id.Should().Be(5);
        result.Title.Should().Be(create.Title);
        result.Rating.Rate.Should().Be(3.9m);
    }

    [Fact(DisplayName = "Get/Update/Delete of an unknown product throw ResourceNotFoundException")]
    public async Task UnknownProduct_Throws()
    {
        _repository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((Product?)null);
        _repository.DeleteAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(false);

        var get = () => new GetProductHandler(_repository, _mapper).Handle(new GetProductQuery(99), CancellationToken.None);
        var update = () => new UpdateProductHandler(_repository, _mapper).Handle(new UpdateProductCommand { Id = 99 }, CancellationToken.None);
        var delete = () => new DeleteProductHandler(_repository).Handle(new DeleteProductCommand(99), CancellationToken.None);

        (await get.Should().ThrowAsync<ResourceNotFoundException>()).Which.Error.Should().Be("Product not found");
        await update.Should().ThrowAsync<ResourceNotFoundException>();
        await delete.Should().ThrowAsync<ResourceNotFoundException>();
    }

    [Fact(DisplayName = "Categories come from the repository")]
    public async Task ListCategories_ReturnsCategories()
    {
        _repository.GetCategoriesAsync(Arg.Any<CancellationToken>()).Returns(new List<string> { "electronics", "jewelery" });

        var result = await new ListCategoriesHandler(_repository).Handle(new ListCategoriesQuery(), CancellationToken.None);

        result.Should().Equal("electronics", "jewelery");
    }

    [Fact(DisplayName = "Product validator rejects invalid data")]
    public void Validator_RejectsInvalidData()
    {
        var command = ValidCommand();
        command.Price = -1;
        command.Title = string.Empty;
        command.Image = "not-an-url";
        command.Rating.Rate = 7;

        var result = new CreateProductValidator().Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(4);
    }

    [Fact(DisplayName = "Product validator accepts valid data")]
    public void Validator_AcceptsValidData()
    {
        new CreateProductValidator().Validate(ValidCommand()).IsValid.Should().BeTrue();
    }
}

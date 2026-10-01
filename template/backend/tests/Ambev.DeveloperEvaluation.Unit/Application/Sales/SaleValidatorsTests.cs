using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Unit.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class SaleValidatorsTests
{
    private readonly CreateSaleValidator _createValidator = new();
    private readonly UpdateSaleValidator _updateValidator = new();

    [Fact(DisplayName = "A valid create command passes")]
    public void Create_Valid()
    {
        _createValidator.Validate(SaleTestData.ValidCreateCommand()).IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Required fields are validated")]
    public void Create_RequiredFields()
    {
        var result = _createValidator.Validate(new CreateSaleCommand());

        result.IsValid.Should().BeFalse();
        result.Errors.Select(e => e.PropertyName).Should().Contain(new[]
        {
            nameof(CreateSaleCommand.SaleNumber),
            nameof(CreateSaleCommand.CustomerId),
            nameof(CreateSaleCommand.CustomerName),
            nameof(CreateSaleCommand.BranchId),
            nameof(CreateSaleCommand.BranchName),
            nameof(CreateSaleCommand.Items)
        });
    }

    [Theory(DisplayName = "Item quantity must be between 1 and 20")]
    [InlineData(0)]
    [InlineData(21)]
    public void Create_InvalidQuantity(int quantity)
    {
        var command = SaleTestData.ValidCreateCommand(items: 1);
        command.Items[0].Quantity = quantity;

        _createValidator.Validate(command).IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "More than 20 identical items across lines is not allowed")]
    public void Create_IdenticalItemsAboveLimit()
    {
        var command = SaleTestData.ValidCreateCommand(items: 1);
        var productId = command.Items[0].ProductId;
        command.Items = new List<SaleItemInput>
        {
            new() { ProductId = productId, ProductName = "Beer", Quantity = 15, UnitPrice = 5m },
            new() { ProductId = productId, ProductName = "Beer", Quantity = 6, UnitPrice = 5m }
        };

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Beer"));
    }

    [Fact(DisplayName = "Unit price must be positive")]
    public void Create_InvalidUnitPrice()
    {
        var command = SaleTestData.ValidCreateCommand(items: 1);
        command.Items[0].UnitPrice = 0;

        _createValidator.Validate(command).IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "A valid update command passes")]
    public void Update_Valid()
    {
        _updateValidator.Validate(SaleTestData.ValidUpdateCommand(Guid.NewGuid())).IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "Update requires the sale id")]
    public void Update_RequiresId()
    {
        var result = _updateValidator.Validate(SaleTestData.ValidUpdateCommand(Guid.Empty));

        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSaleCommand.Id));
    }
}

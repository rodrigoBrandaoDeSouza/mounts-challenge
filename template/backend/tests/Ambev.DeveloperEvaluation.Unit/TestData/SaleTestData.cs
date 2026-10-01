using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.TestData;

/// <summary>
/// Bogus fakers for the sale tests.
/// </summary>
public static class SaleTestData
{
    private static readonly Faker<SaleItemInput> ItemInputFaker = new Faker<SaleItemInput>()
        .RuleFor(i => i.ProductId, _ => Guid.NewGuid())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 20))
        .RuleFor(i => i.UnitPrice, f => Math.Round(f.Random.Decimal(1, 100), 2));

    private static readonly Faker<SaleItem> ItemFaker = new Faker<SaleItem>()
        .RuleFor(i => i.Id, _ => Guid.NewGuid())
        .RuleFor(i => i.ProductId, _ => Guid.NewGuid())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 20))
        .RuleFor(i => i.UnitPrice, f => Math.Round(f.Random.Decimal(1, 100), 2));

    public static CreateSaleCommand ValidCreateCommand(int items = 2)
    {
        var faker = new Faker();
        return new CreateSaleCommand
        {
            SaleNumber = $"S-{faker.Random.AlphaNumeric(8).ToUpperInvariant()}",
            Date = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = faker.Company.CompanyName(),
            BranchId = Guid.NewGuid(),
            BranchName = faker.Address.City(),
            Items = ItemInputFaker.Generate(items)
        };
    }

    public static UpdateSaleCommand ValidUpdateCommand(Guid id, int items = 2)
    {
        var create = ValidCreateCommand(items);
        return new UpdateSaleCommand
        {
            Id = id,
            SaleNumber = create.SaleNumber,
            Date = create.Date,
            CustomerId = create.CustomerId,
            CustomerName = create.CustomerName,
            BranchId = create.BranchId,
            BranchName = create.BranchName,
            Items = create.Items
        };
    }

    public static Sale Sale(int items = 2)
    {
        var faker = new Faker();
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = $"S-{faker.Random.AlphaNumeric(8).ToUpperInvariant()}",
            Date = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = faker.Company.CompanyName(),
            BranchId = Guid.NewGuid(),
            BranchName = faker.Address.City(),
            Items = ItemFaker.Generate(items)
        };

        sale.RecalculateTotals();
        return sale;
    }

    public static SaleItem Item(Guid productId, int quantity, decimal unitPrice) => new()
    {
        Id = Guid.NewGuid(),
        ProductId = productId,
        ProductName = "Product " + productId.ToString()[..4],
        Quantity = quantity,
        UnitPrice = unitPrice
    };
}

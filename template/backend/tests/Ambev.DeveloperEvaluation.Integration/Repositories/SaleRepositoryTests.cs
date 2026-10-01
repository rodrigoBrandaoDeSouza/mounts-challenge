using Ambev.DeveloperEvaluation.Application.Common.Querying;
using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

public class SaleRepositoryTests
{
    private readonly ContextFactory _factory = new();

    private static Sale NewSale(string number, decimal unitPrice, DateTime date, string customer = "Customer", bool cancelled = false)
    {
        var sale = new Sale
        {
            SaleNumber = number,
            Date = date,
            CustomerId = Guid.NewGuid(),
            CustomerName = customer,
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            Cancelled = cancelled,
            Items = new List<SaleItem>
            {
                new() { ProductId = Guid.NewGuid(), ProductName = "Beer", Quantity = 2, UnitPrice = unitPrice }
            }
        };
        sale.RecalculateTotals();
        return sale;
    }

    private async Task SeedAsync(params Sale[] sales)
    {
        await using var context = _factory.Create();
        var repository = new SaleRepository(context);
        foreach (var sale in sales)
            await repository.CreateAsync(sale);
    }

    [Fact(DisplayName = "Create and GetById round-trip the sale with its items")]
    public async Task CreateAndGet()
    {
        var sale = NewSale("S-1", 10m, DateTime.UtcNow);
        await SeedAsync(sale);

        await using var context = _factory.Create();
        var found = await new SaleRepository(context).GetByIdAsync(sale.Id);

        found.Should().NotBeNull();
        found!.Items.Should().ContainSingle();
        found.TotalAmount.Should().Be(20m);
    }

    [Fact(DisplayName = "List applies filters, ordering and pagination")]
    public async Task List_FiltersOrdersAndPaginates()
    {
        var now = DateTime.UtcNow;
        await SeedAsync(
            NewSale("S-1", 10m, now.AddDays(-3), "Bar do Zé"),
            NewSale("S-2", 30m, now.AddDays(-2), "Bar Central"),
            NewSale("S-3", 20m, now.AddDays(-1), "Mercado"),
            NewSale("S-4", 40m, now, "Bar Novo", cancelled: true));

        var query = new ListSalesQuery { Order = "totalAmount desc", Size = 2 };
        query.Filters["customerName"] = new[] { "bar*" };
        query.Filters["cancelled"] = new[] { "false" };
        var options = QueryOptionsParser.Parse(query, SaleQueryFields.Map);

        await using var context = _factory.Create();
        var page = await new SaleRepository(context).ListAsync(options);

        page.TotalItems.Should().Be(2);
        page.TotalPages.Should().Be(1);
        page.Items.Select(s => s.SaleNumber).Should().Equal("S-2", "S-1");
    }

    [Fact(DisplayName = "List supports _min/_max ranges")]
    public async Task List_Ranges()
    {
        var now = DateTime.UtcNow;
        await SeedAsync(
            NewSale("S-1", 10m, now),
            NewSale("S-2", 30m, now),
            NewSale("S-3", 50m, now));

        var query = new ListSalesQuery();
        query.Filters["_minTotalAmount"] = new[] { "40" };
        query.Filters["_maxTotalAmount"] = new[] { "80" };
        var options = QueryOptionsParser.Parse(query, SaleQueryFields.Map);

        await using var context = _factory.Create();
        var page = await new SaleRepository(context).ListAsync(options);

        page.Items.Select(s => s.SaleNumber).Should().BeEquivalentTo(new[] { "S-2" });
    }

    [Fact(DisplayName = "Update synchronizes the items of the sale")]
    public async Task Update_SynchronizesItems()
    {
        var sale = NewSale("S-1", 10m, DateTime.UtcNow);
        await SeedAsync(sale);

        await using (var context = _factory.Create())
        {
            var repository = new SaleRepository(context);
            var tracked = (await repository.GetByIdAsync(sale.Id))!;
            tracked.ReplaceItems(new[]
            {
                new SaleItem { ProductId = Guid.NewGuid(), ProductName = "Soda", Quantity = 4, UnitPrice = 5m }
            });
            await repository.UpdateAsync(tracked);
        }

        await using (var context = _factory.Create())
        {
            var reloaded = (await new SaleRepository(context).GetByIdAsync(sale.Id))!;
            reloaded.Items.Should().ContainSingle().Which.ProductName.Should().Be("Soda");
            reloaded.TotalAmount.Should().Be(18m);
        }
    }

    [Fact(DisplayName = "SaleNumberExists ignores the informed sale")]
    public async Task SaleNumberExists()
    {
        var sale = NewSale("S-1", 10m, DateTime.UtcNow);
        await SeedAsync(sale);

        await using var context = _factory.Create();
        var repository = new SaleRepository(context);

        (await repository.SaleNumberExistsAsync("S-1")).Should().BeTrue();
        (await repository.SaleNumberExistsAsync("S-1", sale.Id)).Should().BeFalse();
        (await repository.SaleNumberExistsAsync("S-2")).Should().BeFalse();
    }

    [Fact(DisplayName = "Delete removes the sale")]
    public async Task Delete()
    {
        var sale = NewSale("S-1", 10m, DateTime.UtcNow);
        await SeedAsync(sale);

        await using var context = _factory.Create();
        var repository = new SaleRepository(context);

        (await repository.DeleteAsync(sale.Id)).Should().BeTrue();
        (await repository.DeleteAsync(sale.Id)).Should().BeFalse();
        (await repository.GetByIdAsync(sale.Id)).Should().BeNull();
    }
}

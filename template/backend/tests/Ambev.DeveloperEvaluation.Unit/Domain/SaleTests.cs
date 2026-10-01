using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Unit.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain;

public class SaleTests
{
    [Fact(DisplayName = "Cancel marks the sale as cancelled")]
    public void Cancel_MarksSaleAsCancelled()
    {
        var sale = SaleTestData.Sale();

        sale.Cancel();

        sale.Cancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "Cancelling a cancelled sale is not allowed")]
    public void Cancel_AlreadyCancelled_Throws()
    {
        var sale = SaleTestData.Sale();
        sale.Cancel();

        var act = () => sale.Cancel();

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "CancelItem cancels the item and recalculates the total")]
    public void CancelItem_RecalculatesTotals()
    {
        var beer = Guid.NewGuid();
        var soda = Guid.NewGuid();
        var sale = new Sale
        {
            Items = new List<SaleItem>
            {
                SaleTestData.Item(beer, 4, 10m),
                SaleTestData.Item(soda, 1, 5m)
            }
        };
        sale.RecalculateTotals();
        sale.TotalAmount.Should().Be(41m);

        var item = sale.CancelItem(sale.Items[0].Id);

        item.Cancelled.Should().BeTrue();
        sale.TotalAmount.Should().Be(5m);
    }

    [Fact(DisplayName = "CancelItem with an unknown item throws ResourceNotFoundException")]
    public void CancelItem_UnknownItem_Throws()
    {
        var sale = SaleTestData.Sale();

        var act = () => sale.CancelItem(Guid.NewGuid());

        act.Should().Throw<ResourceNotFoundException>();
    }

    [Fact(DisplayName = "CancelItem twice is not allowed")]
    public void CancelItem_AlreadyCancelled_Throws()
    {
        var sale = SaleTestData.Sale();
        var itemId = sale.Items[0].Id;
        sale.CancelItem(itemId);

        var act = () => sale.CancelItem(itemId);

        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "ReplaceItems updates, adds and removes items")]
    public void ReplaceItems_SynchronizesItems()
    {
        var sale = SaleTestData.Sale(items: 2);
        var kept = sale.Items[0];
        var removedId = sale.Items[1].Id;
        var newProduct = Guid.NewGuid();

        sale.ReplaceItems(new[]
        {
            new SaleItem { Id = kept.Id, ProductId = kept.ProductId, ProductName = kept.ProductName, Quantity = 5, UnitPrice = 2m },
            new SaleItem { ProductId = newProduct, ProductName = "New", Quantity = 1, UnitPrice = 3m }
        });

        sale.Items.Should().HaveCount(2);
        sale.Items.Should().NotContain(i => i.Id == removedId);
        sale.Items.Single(i => i.Id == kept.Id).Quantity.Should().Be(5);
        sale.Items.Should().Contain(i => i.ProductId == newProduct && i.Id == Guid.Empty);
        sale.TotalAmount.Should().Be(5 * 2m * 0.9m + 3m);
    }

    [Fact(DisplayName = "A cancelled sale cannot be modified")]
    public void ReplaceItems_CancelledSale_Throws()
    {
        var sale = SaleTestData.Sale();
        sale.Cancel();

        var act = () => sale.ReplaceItems(Array.Empty<SaleItem>());

        act.Should().Throw<DomainException>();
    }
}

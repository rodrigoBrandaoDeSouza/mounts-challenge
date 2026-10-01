using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Policies;
using Ambev.DeveloperEvaluation.Unit.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain;

public class SaleDiscountPolicyTests
{
    [Theory(DisplayName = "Discount tiers by quantity of identical items")]
    [InlineData(1, 0)]
    [InlineData(3, 0)]
    [InlineData(4, 10)]
    [InlineData(9, 10)]
    [InlineData(10, 20)]
    [InlineData(20, 20)]
    public void GetDiscountPercent_ReturnsTheTierDiscount(int quantity, decimal expected)
    {
        SaleDiscountPolicy.GetDiscountPercent(quantity).Should().Be(expected);
    }

    [Fact(DisplayName = "It's not possible to sell above 20 identical items")]
    public void GetDiscountPercent_AboveLimit_Throws()
    {
        var act = () => SaleDiscountPolicy.GetDiscountPercent(21);
        act.Should().Throw<DomainException>();
    }

    [Fact(DisplayName = "Item totals and sale total are calculated with the discount")]
    public void Apply_CalculatesItemAndSaleTotals()
    {
        var beer = Guid.NewGuid();
        var soda = Guid.NewGuid();
        var sale = new Sale
        {
            Items = new List<SaleItem>
            {
                SaleTestData.Item(beer, 12, 9.90m),
                SaleTestData.Item(soda, 2, 11.50m)
            }
        };

        SaleDiscountPolicy.Apply(sale);

        sale.Items[0].DiscountPercent.Should().Be(20m);
        sale.Items[0].TotalPrice.Should().Be(95.04m);
        sale.Items[1].DiscountPercent.Should().Be(0m);
        sale.Items[1].TotalPrice.Should().Be(23.00m);
        sale.TotalAmount.Should().Be(118.04m);
    }

    [Fact(DisplayName = "Identical items informed in several lines are added up")]
    public void Apply_GroupsLinesOfTheSameProduct()
    {
        var beer = Guid.NewGuid();
        var sale = new Sale
        {
            Items = new List<SaleItem>
            {
                SaleTestData.Item(beer, 2, 5m),
                SaleTestData.Item(beer, 3, 5m)
            }
        };

        SaleDiscountPolicy.Apply(sale);

        sale.Items.Should().OnlyContain(i => i.DiscountPercent == 10m);
        sale.TotalAmount.Should().Be(22.50m);
    }

    [Fact(DisplayName = "More than 20 identical items across lines is not allowed")]
    public void Apply_AboveLimitAcrossLines_Throws()
    {
        var beer = Guid.NewGuid();
        var sale = new Sale
        {
            Items = new List<SaleItem>
            {
                SaleTestData.Item(beer, 15, 5m),
                SaleTestData.Item(beer, 6, 5m)
            }
        };

        var act = () => SaleDiscountPolicy.Apply(sale);

        act.Should().Throw<DomainException>().WithMessage("*20 identical items*");
    }

    [Fact(DisplayName = "Cancelled items are ignored in discounts and totals")]
    public void Apply_IgnoresCancelledItems()
    {
        var beer = Guid.NewGuid();
        var cancelled = SaleTestData.Item(beer, 10, 5m);
        cancelled.Cancelled = true;

        var sale = new Sale
        {
            Items = new List<SaleItem> { cancelled, SaleTestData.Item(beer, 3, 5m) }
        };

        SaleDiscountPolicy.Apply(sale);

        sale.Items[1].DiscountPercent.Should().Be(0m);
        sale.TotalAmount.Should().Be(15m);
    }
}

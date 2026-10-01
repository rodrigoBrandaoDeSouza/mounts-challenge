using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Policies;

/// <summary>
/// Quantity-based discount rules of a sale.
/// </summary>
/// <remarks>
/// Identical items are the (non cancelled) items of the same product (External Identity <c>ProductId</c>):
/// <list type="bullet">
/// <item>below 4 identical items: no discount;</item>
/// <item>4 to 9 identical items: 10% discount;</item>
/// <item>10 to 20 identical items: 20% discount;</item>
/// <item>above 20 identical items: not allowed.</item>
/// </list>
/// </remarks>
public static class SaleDiscountPolicy
{
    public const int MaxIdenticalItems = 20;
    public const int MinItemsForDiscount = 4;
    public const int MinItemsForHigherDiscount = 10;
    public const decimal Discount = 10m;
    public const decimal HigherDiscount = 20m;

    /// <summary>
    /// Returns the discount percentage for the given quantity of identical items.
    /// </summary>
    /// <exception cref="DomainException">When the quantity exceeds <see cref="MaxIdenticalItems"/>.</exception>
    public static decimal GetDiscountPercent(int quantity)
    {
        if (quantity > MaxIdenticalItems)
            throw new DomainException($"It's not possible to sell above {MaxIdenticalItems} identical items.");

        if (quantity >= MinItemsForHigherDiscount)
            return HigherDiscount;

        if (quantity >= MinItemsForDiscount)
            return Discount;

        return 0m;
    }

    /// <summary>
    /// Applies the discount rules to every active item and recalculates the sale total.
    /// Cancelled items keep their values but are not considered in the discounts nor in the total.
    /// </summary>
    public static void Apply(Sale sale)
    {
        var activeGroups = sale.Items
            .Where(i => !i.Cancelled)
            .GroupBy(i => i.ProductId);

        foreach (var group in activeGroups)
        {
            var quantity = group.Sum(i => i.Quantity);

            if (quantity > MaxIdenticalItems)
            {
                var productName = group.First().ProductName;
                throw new DomainException(
                    $"It's not possible to sell above {MaxIdenticalItems} identical items. Product '{productName}' has {quantity} units.");
            }

            var discountPercent = GetDiscountPercent(quantity);

            foreach (var item in group)
                item.ApplyDiscount(discountPercent);
        }

        sale.TotalAmount = sale.Items
            .Where(i => !i.Cancelled)
            .Sum(i => i.TotalPrice);
    }
}

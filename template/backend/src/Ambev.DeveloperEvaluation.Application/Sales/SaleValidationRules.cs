using Ambev.DeveloperEvaluation.Domain.Policies;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales;

/// <summary>
/// Validation rules shared by the create and update sale commands.
/// </summary>
public static class SaleValidationRules
{
    public const int SaleNumberMaxLength = 50;
    public const int NameMaxLength = 100;

    public static void ConfigureItemRules(InlineValidator<SaleItemInput> item)
    {
        item.RuleFor(i => i.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        item.RuleFor(i => i.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(NameMaxLength);

        item.RuleFor(i => i.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
            .LessThanOrEqualTo(SaleDiscountPolicy.MaxIdenticalItems)
            .WithMessage($"It's not possible to sell above {SaleDiscountPolicy.MaxIdenticalItems} identical items.");

        item.RuleFor(i => i.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than zero.");
    }

    /// <summary>
    /// Identical items (same product) informed in more than one line are added up.
    /// </summary>
    public static IEnumerable<string> ProductsAboveLimit(IEnumerable<SaleItemInput> items) =>
        items
            .GroupBy(i => i.ProductId)
            .Where(g => g.Sum(i => i.Quantity) > SaleDiscountPolicy.MaxIdenticalItems)
            .Select(g => g.First().ProductName.Trim());
}

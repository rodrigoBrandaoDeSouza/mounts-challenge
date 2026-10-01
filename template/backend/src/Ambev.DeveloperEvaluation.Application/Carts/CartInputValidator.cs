using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Carts;

/// <summary>
/// Validation rules shared by the create and update cart commands.
/// </summary>
public abstract class CartInputValidator<T> : AbstractValidator<T> where T : CartInput
{
    protected CartInputValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than zero.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");

        RuleFor(x => x.Products)
            .NotEmpty().WithMessage("At least one product is required.");

        RuleForEach(x => x.Products).ChildRules(product =>
        {
            product.RuleFor(p => p.ProductId).GreaterThan(0).WithMessage("Product ID must be greater than zero.");
            product.RuleFor(p => p.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });

        RuleFor(x => x.Products)
            .Must(products => products.Select(p => p.ProductId).Distinct().Count() == products.Count)
            .When(x => x.Products is not null)
            .WithMessage("Each product can be informed only once per cart.");
    }
}

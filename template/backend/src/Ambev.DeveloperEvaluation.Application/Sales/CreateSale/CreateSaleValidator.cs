using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleValidator()
    {
        RuleFor(x => x.SaleNumber)
            .NotEmpty().WithMessage("Sale number is required.")
            .MaximumLength(SaleValidationRules.SaleNumberMaxLength);

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(SaleValidationRules.NameMaxLength);

        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Branch ID is required.");

        RuleFor(x => x.BranchName)
            .NotEmpty().WithMessage("Branch name is required.")
            .MaximumLength(SaleValidationRules.NameMaxLength);

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one sale item is required.");

        RuleForEach(x => x.Items).ChildRules(SaleValidationRules.ConfigureItemRules);

        RuleFor(x => x.Items)
            .Custom((items, context) =>
            {
                foreach (var product in SaleValidationRules.ProductsAboveLimit(items))
                    context.AddFailure("Items", $"The product '{product}' exceeds the maximum allowed quantity of 20 identical items.");
            });
    }
}

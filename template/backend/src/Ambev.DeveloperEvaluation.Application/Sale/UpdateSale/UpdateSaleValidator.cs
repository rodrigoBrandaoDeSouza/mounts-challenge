using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sale.UpdateSale
{
    /// <summary>
    /// Validator for <see cref="UpdateSaleCommand"/>.
    /// </summary>
    /// <remarks>
    /// Ensures that the update command contains valid data before processing.
    /// </remarks>
    public class UpdateSaleValidator : AbstractValidator<UpdateSaleCommand>
    {
        public UpdateSaleValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Sale ID is required for update.");

            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Customer ID is required.");

            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("Customer name is required.")
                .MaximumLength(100);

            RuleFor(x => x.BranchId)
                .NotEmpty().WithMessage("Branch ID is required.");

            RuleFor(x => x.BranchName)
                .NotEmpty().WithMessage("Branch name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("At least one sale item is required.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId)
                    .NotEmpty().WithMessage("Product ID is required.");

                item.RuleFor(i => i.ProductName)
                    .NotEmpty().WithMessage("Product name is required.")
                    .MaximumLength(100);

                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
                    .LessThanOrEqualTo(20).WithMessage("Quantity cannot exceed 20.");

                item.RuleFor(i => i.UnitPrice)
                    .GreaterThan(0).WithMessage("Unit price must be greater than zero.");
            });

            RuleFor(x => x)
                .Custom((command, context) =>
                    {
                        var grouped = command.Items
                            // Identical items = same product (External Identity)
                            .GroupBy(i => i.ProductId)
                            .Select(g => new { Product = g.First().ProductName.Trim(), TotalQuantity = g.Sum(i => i.Quantity) });

                        foreach (var group in grouped)
                        {
                            if (group.TotalQuantity > 20)
                            {
                                context.AddFailure($"The product '{group.Product}' exceeds the maximum allowed quantity of 20 units.");
                            }
                        }
                    });
        }
    }
}

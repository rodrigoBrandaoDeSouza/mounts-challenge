using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sale.DeleteSale
{
    /// <summary>
    /// Validator for <see cref="DeleteSaleCommand"/>.
    /// </summary>
    /// <remarks>
    /// This validator ensures that a valid sale identifier is provided
    /// before attempting to delete a sale.
    /// </remarks>
    public class DeleteSaleValidator : AbstractValidator<DeleteSaleCommand>
    {
        public DeleteSaleValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Sale ID is required for deletion.");
        }
    }
}

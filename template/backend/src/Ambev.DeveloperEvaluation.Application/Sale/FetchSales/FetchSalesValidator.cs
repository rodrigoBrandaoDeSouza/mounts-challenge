using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sale.FetchSales
{
    /// <summary>
    /// Validator for <see cref="FetchSalesCommand"/>.
    /// </summary>
    /// <remarks>
    /// As this operation retrieves all sales, validation is minimal.
    /// This class exists for consistency and future extensibility.
    /// </remarks>
    public class FetchSalesValidator : AbstractValidator<FetchSalesCommand>
    {
        public FetchSalesValidator()
        {
            // No rules currently required; placeholder for potential future filters.
        }
    }
}

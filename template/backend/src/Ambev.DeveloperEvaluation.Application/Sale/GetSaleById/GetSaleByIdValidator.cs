using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sale.GetSale
{
    /// <summary>
    /// Validator for <see cref="GetSaleByIdCommand"/>.
    /// </summary>
    /// <remarks>
    /// Ensures that a valid sale ID is provided before retrieving sale data.
    /// </remarks>
    public class GetSaleByIdValidator : AbstractValidator<GetSaleByIdCommand>
    {
        public GetSaleByIdValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Sale ID is required.");
        }
    }
}

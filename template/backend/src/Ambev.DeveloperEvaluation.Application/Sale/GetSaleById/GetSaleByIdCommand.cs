using Ambev.DeveloperEvaluation.Application.Models;
using Ambev.DeveloperEvaluation.Common.Validation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sale.GetSale
{
    /// <summary>
    /// Command for retrieving a sale by its unique identifier.
    /// </summary>
    /// <remarks>
    /// This command is used to request the details of a specific sale.
    /// It implements <see cref="IRequest{TResponse}"/> to return a 
    /// <see cref=" OperationResult<Domain.Entities.Sale>"/> when executed.
    /// 
    /// The data is validated using the <see cref="GetSaleByIdValidator"/>,
    /// ensuring that a valid sale identifier is provided.
    /// </remarks>
    public class GetSaleByIdCommand : IRequest<OperationResult<Domain.Entities.Sale>>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the sale to retrieve.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Validates the command using FluentValidation.
        /// </summary>
        /// <returns>
        /// A <see cref="ValidationResultDetail"/> containing validation status 
        /// and any errors found.
        /// </returns>
        public ValidationResultDetail Validate()
        {
            var validator = new GetSaleByIdValidator();
            var result = validator.Validate(this);
            return new ValidationResultDetail
            {
                IsValid = result.IsValid,
                Errors = result.Errors.Select(e => (ValidationErrorDetail)e)
            };
        }
    }
}

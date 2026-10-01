using Ambev.DeveloperEvaluation.Application.Models;
using Ambev.DeveloperEvaluation.Common.Validation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sale.DeleteSale
{
    /// <summary>
    /// Command for deleting an existing sale.
    /// </summary>
    /// <remarks>
    /// This command contains the necessary information to request the deletion of a sale.
    /// It implements <see cref="IRequest{TResponse}"/> to trigger the delete operation 
    /// and return a <see cref="DeleteSaleResponse"/> after processing.
    /// 
    /// Validation of this command is handled by <see cref="DeleteSaleValidator"/>,
    /// which ensures that the provided data is valid before deletion occurs.
    /// </remarks>
    public class DeleteSaleCommand : IRequest<OperationResult<Domain.Entities.Sale>>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the sale to be deleted.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Validates the command using FluentValidation.
        /// </summary>
        /// <returns>
        /// A <see cref="ValidationResultDetail"/> containing validation status 
        /// and any potential validation errors.
        /// </returns>
        public ValidationResultDetail Validate()
        {
            var validator = new DeleteSaleValidator();
            var result = validator.Validate(this);
            return new ValidationResultDetail
            {
                IsValid = result.IsValid,
                Errors = result.Errors.Select(e => (ValidationErrorDetail)e)
            };
        }
    }
}

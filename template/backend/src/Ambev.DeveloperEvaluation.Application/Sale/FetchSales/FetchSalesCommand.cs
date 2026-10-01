using Ambev.DeveloperEvaluation.Application.Models;
using Ambev.DeveloperEvaluation.Common.Validation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sale.FetchSales
{
    /// <summary>
    /// Command for retrieving all sales records.
    /// </summary>
    /// <remarks>
    /// This command triggers the retrieval of all sales stored in the system.
    /// It implements <see cref="IRequest{TResponse}"/> to return a 
    /// <see cref="FetchSalesResponse"/> when processed.
    /// 
    /// The command may be extended in the future to include filters such as
    /// date ranges, branch, or customer information.
    /// 
    /// Validation is performed by <see cref="FetchSalesValidator"/>.
    /// </remarks>
    public class FetchSalesCommand : IRequest<OperationResult<List<Domain.Entities.Sale>>>
    {
        /// <summary>
        /// Gets or sets the number of page.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the size of page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Validates the command using FluentValidation.
        /// </summary>
        /// <returns>
        /// A <see cref="ValidationResultDetail"/> containing validation status 
        /// and potential errors found.
        /// </returns>
        public ValidationResultDetail Validate()
        {
            var validator = new FetchSalesValidator();
            var result = validator.Validate(this);
            return new ValidationResultDetail
            {
                IsValid = result.IsValid,
                Errors = result.Errors.Select(e => (ValidationErrorDetail)e)
            };
        }
    }
}

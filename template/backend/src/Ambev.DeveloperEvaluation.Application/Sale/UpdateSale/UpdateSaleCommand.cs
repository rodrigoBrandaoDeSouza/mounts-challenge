using Ambev.DeveloperEvaluation.Application.Models;
using Ambev.DeveloperEvaluation.Common.Validation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sale.UpdateSale
{
    /// <summary>
    /// Command for updating an existing sale.
    /// </summary>
    /// <remarks>
    /// This command captures all information required to update an existing sale, 
    /// including customer data, branch, and sale items.
    /// 
    /// It implements <see cref="IRequest{TResponse}"/> to initiate a request 
    /// that returns a <see cref="OperationResult<Domain.Entities.Sale>"/> upon completion.
    /// 
    /// Validation is handled by <see cref="UpdateSaleValidator"/>,
    /// ensuring that the data provided is valid before the update operation.
    /// </remarks>
    public class UpdateSaleCommand : IRequest<OperationResult<Domain.Entities.Sale>>
    {
        /// <summary>
        /// Gets or sets the number of sale.
        /// </summary>
        public string SaleNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unique identifier of the sale to be updated.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the date when the sale was made. When omitted, the original sale date is kept.
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Gets or sets the external identity of the customer (Customers bounded context).
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the denormalized name of the customer who made the purchase.
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the external identity of the branch (Branches bounded context).
        /// </summary>
        public Guid BranchId { get; set; }

        /// <summary>
        /// Gets or sets the denormalized name of the branch where the sale was performed.
        /// </summary>
        public string BranchName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total amount of sale.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the list of sale items to be updated.
        /// </summary>
        public List<UpdateSaleItemDto> Items { get; set; } = new();

        /// <summary>
        /// Validates the command using FluentValidation.
        /// </summary>
        /// <returns>
        /// A <see cref="ValidationResultDetail"/> with the validation status and possible errors.
        /// </returns>
        public ValidationResultDetail Validate()
        {
            var validator = new UpdateSaleValidator();
            var result = validator.Validate(this);
            return new ValidationResultDetail
            {
                IsValid = result.IsValid,
                Errors = result.Errors.Select(e => (ValidationErrorDetail)e)
            };
        }
    }

    /// <summary>
    /// DTO representing a sale item in the update operation.
    /// </summary>
    public class UpdateSaleItemDto
    {
        /// <summary>
        /// Identifier of an existing item of the sale. When null (or unknown), the item is added as a new one.
        /// Existing items that are not sent in the request are removed from the sale.
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// External identity of the product (Products bounded context).
        /// </summary>
        public Guid ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
        public bool Cancelled { get; set; }

    }
}

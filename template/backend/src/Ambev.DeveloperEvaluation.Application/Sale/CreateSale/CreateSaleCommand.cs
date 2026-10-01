using Ambev.DeveloperEvaluation.Application.Models;
using Ambev.DeveloperEvaluation.Common.Validation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sale.CreateSale
{
    /// <summary>
    /// Command for creating a new sale.
    /// </summary>
    /// <remarks>
    /// This command is responsible for capturing all required data to create a sale,
    /// including the customer information, branch where the sale occurred,
    /// and the list of items associated with the sale.
    /// 
    /// It implements <see cref="IRequest{TResponse}"/> to initiate a request
    /// that returns a <see cref="CreateSaleResult"/> after processing.
    /// 
    /// The data provided in this command is validated through the
    /// <see cref="CreateSaleValidator"/>, which uses <see cref="FluentValidation.AbstractValidator{T}"/>
    /// to ensure that the fields are properly populated and follow the defined business rules.
    /// </remarks>
    public class CreateSaleCommand : IRequest<OperationResult<Domain.Entities.Sale>>
    {
        /// <summary>
        /// Gets or sets the number of sale.
        /// </summary>
        public string SaleNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date when the sale was made. When omitted, the current UTC date/time is used.
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
        /// Gets or sets the status of sale.
        /// </summary>
        public bool Cancelled { get; set; }

        /// <summary>
        /// Gets or sets the list of items sold in this sale.
        /// </summary>
        public List<CreateSaleItemDto> Items { get; set; } = new();
    
    }

    /// <summary>
    /// Data transfer object representing an item in the sale being created.
    /// </summary>
    /// <remarks>
    /// Each item represents a product being sold, including its name, quantity, and unit price.
    /// Validation for this DTO is handled as part of <see cref="CreateSaleValidator"/>.
    /// </remarks>
    public class CreateSaleItemDto
    {
        /// <summary>
        /// Gets or sets the external identity of the product (Products bounded context).
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Gets or sets the denormalized name of the product being sold.
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the quantity of the product in the sale.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the price of a single unit of the product.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the total price of a single unit of the product.
        /// </summary>
        public decimal Total { get; set; }


        /// <summary>
        /// Gets or sets the status of the item.
        /// </summary>
        public bool Cancelled { get; set; }
    }
}

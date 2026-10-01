using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales
{
    /// <summary>
    /// Sales records (complete CRUD plus cancellation of sales and items).
    /// </summary>
    [Authorize]
    [Route("api/sales")]
    public class SalesController : BaseController
    {
        private readonly IMediator _mediator;

        public SalesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Retrieves the sales (pagination: _page, _size; ordering: _order; filters: field=value, _minField, _maxField).</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<SaleResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> List(
            [FromQuery(Name = "_page")] int? page,
            [FromQuery(Name = "_size")] int? size,
            [FromQuery(Name = "_order")] string? order,
            CancellationToken cancellationToken)
        {
            var query = BuildListQuery<ListSalesQuery>(page, size, order);
            return OkPaginated(await _mediator.Send(query, cancellationToken));
        }

        /// <summary>Retrieves a sale by its identifier.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(SaleResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
            Ok(await _mediator.Send(new GetSaleQuery(id), cancellationToken));

        /// <summary>Creates a sale. Discounts and totals are calculated by the API.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(SaleResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateSaleCommand command, CancellationToken cancellationToken)
        {
            var sale = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = sale.Id }, sale);
        }

        /// <summary>Updates a sale (items with id are updated, items without id are added, missing items are removed).</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(SaleResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSaleCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            return Ok(await _mediator.Send(command, cancellationToken));
        }

        /// <summary>Deletes a sale.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteSaleCommand(id), cancellationToken);
            return Ok(new MessageResponse("Sale deleted successfully"));
        }

        /// <summary>Cancels a sale (publishes SaleCancelled).</summary>
        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(SaleResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken) =>
            Ok(await _mediator.Send(new CancelSaleCommand(id), cancellationToken));

        /// <summary>Cancels an item of a sale (publishes ItemCancelled). Discounts and totals are recalculated.</summary>
        [HttpPatch("{id:guid}/items/{itemId:guid}/cancel")]
        [ProducesResponseType(typeof(SaleResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelItem(Guid id, Guid itemId, CancellationToken cancellationToken) =>
            Ok(await _mediator.Send(new CancelSaleItemCommand(id, itemId), cancellationToken));
    }
}

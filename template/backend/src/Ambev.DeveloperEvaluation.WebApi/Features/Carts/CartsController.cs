using Ambev.DeveloperEvaluation.Application.Carts;
using Ambev.DeveloperEvaluation.Application.Carts.CreateCart;
using Ambev.DeveloperEvaluation.Application.Carts.DeleteCart;
using Ambev.DeveloperEvaluation.Application.Carts.GetCart;
using Ambev.DeveloperEvaluation.Application.Carts.ListCarts;
using Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;
using Ambev.DeveloperEvaluation.WebApi.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Carts;

/// <summary>
/// Shopping carts.
/// </summary>
[Authorize]
[Route("api/carts")]
public class CartsController : BaseController
{
    private readonly IMediator _mediator;

    public CartsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Retrieves the carts (pagination: _page, _size; ordering: _order; filters: field=value, _minField, _maxField).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<CartResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery(Name = "_page")] int? page,
        [FromQuery(Name = "_size")] int? size,
        [FromQuery(Name = "_order")] string? order,
        CancellationToken cancellationToken)
    {
        var query = BuildListQuery<ListCartsQuery>(page, size, order);
        return OkPaginated(await _mediator.Send(query, cancellationToken));
    }

    /// <summary>Retrieves a cart by its identifier.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CartResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetCartQuery(id), cancellationToken));

    /// <summary>Adds a new cart.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CartResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateCartCommand command, CancellationToken cancellationToken)
    {
        var cart = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = cart.Id }, cart);
    }

    /// <summary>Updates a cart (the informed products replace the current ones).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CartResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCartCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>Deletes a cart.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCartCommand(id), cancellationToken);
        return Ok(new MessageResponse("Cart deleted successfully"));
    }
}

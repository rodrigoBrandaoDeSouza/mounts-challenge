using Ambev.DeveloperEvaluation.Application.Products;
using Ambev.DeveloperEvaluation.Application.Products.CreateProduct;
using Ambev.DeveloperEvaluation.Application.Products.DeleteProduct;
using Ambev.DeveloperEvaluation.Application.Products.GetProduct;
using Ambev.DeveloperEvaluation.Application.Products.ListCategories;
using Ambev.DeveloperEvaluation.Application.Products.ListProducts;
using Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;
using Ambev.DeveloperEvaluation.WebApi.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Products;

/// <summary>
/// Products catalog. Reading is public; changes require authentication.
/// </summary>
[Authorize]
[Route("api/products")]
public class ProductsController : BaseController
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Retrieves the products (pagination: _page, _size; ordering: _order; filters: field=value, _minField, _maxField).</summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ProductResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery(Name = "_page")] int? page,
        [FromQuery(Name = "_size")] int? size,
        [FromQuery(Name = "_order")] string? order,
        CancellationToken cancellationToken)
    {
        var query = BuildListQuery<ListProductsQuery>(page, size, order);
        return OkPaginated(await _mediator.Send(query, cancellationToken));
    }

    /// <summary>Retrieves a product by its identifier.</summary>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetProductQuery(id), cancellationToken));

    /// <summary>Retrieves all product categories.</summary>
    [AllowAnonymous]
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new ListCategoriesQuery(), cancellationToken));

    /// <summary>Retrieves the products of a category.</summary>
    [AllowAnonymous]
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(PaginatedResponse<ProductResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListByCategory(
        string category,
        [FromQuery(Name = "_page")] int? page,
        [FromQuery(Name = "_size")] int? size,
        [FromQuery(Name = "_order")] string? order,
        CancellationToken cancellationToken)
    {
        var query = BuildListQuery<ListProductsQuery>(page, size, order);
        query.Filters["category"] = new[] { category };
        return OkPaginated(await _mediator.Send(query, cancellationToken));
    }

    /// <summary>Adds a new product.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>Updates a product.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>Deletes a product.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return Ok(new MessageResponse("Product deleted successfully"));
    }
}

using Ambev.DeveloperEvaluation.Application.Models;
using Ambev.DeveloperEvaluation.Application.Sale.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sale.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sale.FetchSales;
using Ambev.DeveloperEvaluation.Application.Sale.GetSale;
using Ambev.DeveloperEvaluation.Application.Sale.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sale.Requests;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale
{
    /// <summary>
    /// Controller responsible for managing sale-related endpoints.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SaleController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<SaleController> _logger;

        public SaleController(IMediator mediator, IMapper mapper, ILogger<SaleController> logger)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        // POST: api/sale
        [HttpPost]
        [ProducesResponseType(typeof(OperationResult<Domain.Entities.Sale>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateSaleRequest request)
        {
            var command = _mapper.Map<CreateSaleCommand>(request);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }

        // GET: api/sale/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OperationResult<Domain.Entities.Sale>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] GetSaleByIdCommand request)
        {
            var command = _mapper.Map<GetSaleByIdCommand>(request);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // GET: api/fetch
        [HttpGet("Fetch")]
        [ProducesResponseType(typeof(OperationResult<List<Domain.Entities.Sale>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FetchSales([FromQuery] FetchSalesRequest request)
        {
            var command = _mapper.Map<FetchSalesCommand>(request);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // PUT: api/sale/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(OperationResult<Domain.Entities.Sale>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSaleRequest request)
        {
            var command = _mapper.Map<UpdateSaleCommand>(request);
            command.Id = id;
            var result = await _mediator.Send(command);

            if (result.Success)
                return Ok(result);
            else
                return NotFound(result);

        }

        // DELETE: api/sale/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(OperationResult<Domain.Entities.Sale>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] DeleteSaleRequest request)
        {
            var command = _mapper.Map<DeleteSaleCommand>(request);
            var result = await _mediator.Send(command);

            if (result.Success)
                return NoContent();
            else
                return NotFound(result);
        }
    }
}
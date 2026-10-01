using Ambev.DeveloperEvaluation.Application.Users;
using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Application.Users.ListUsers;
using Ambev.DeveloperEvaluation.Application.Users.UpdateUser;
using Ambev.DeveloperEvaluation.WebApi.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users;

/// <summary>
/// Users. Registration (POST) is public; the other operations require authentication.
/// </summary>
[Authorize]
[Route("api/users")]
public class UsersController : BaseController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Retrieves the users (pagination: _page, _size; ordering: _order; filters: field=value, _minField, _maxField).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<UserResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery(Name = "_page")] int? page,
        [FromQuery(Name = "_size")] int? size,
        [FromQuery(Name = "_order")] string? order,
        CancellationToken cancellationToken)
    {
        var query = BuildListQuery<ListUsersQuery>(page, size, order);
        return OkPaginated(await _mediator.Send(query, cancellationToken));
    }

    /// <summary>Retrieves a user by its identifier.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetUserQuery(id), cancellationToken));

    /// <summary>Adds a new user.</summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(UserResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>Updates a user (when the password is omitted the current one is kept).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UserResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>Deletes a user, returning the deleted user.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(UserResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new DeleteUserCommand(id), cancellationToken));
}

using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.DeleteUser;

/// <summary>
/// Deletes a user, returning the deleted user (as defined in the Users API).
/// </summary>
public record DeleteUserCommand(int Id) : IRequest<UserResult>;

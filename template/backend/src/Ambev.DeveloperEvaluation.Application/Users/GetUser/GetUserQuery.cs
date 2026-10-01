using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.GetUser;

public record GetUserQuery(int Id) : IRequest<UserResult>;

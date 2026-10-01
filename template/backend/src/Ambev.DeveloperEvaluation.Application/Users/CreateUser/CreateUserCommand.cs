using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.CreateUser;

public class CreateUserCommand : UserInput, IRequest<UserResult>
{
}

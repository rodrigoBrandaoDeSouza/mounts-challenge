using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;

/// <summary>
/// Authenticates a user (POST /auth/login).
/// </summary>
public class AuthenticateUserCommand : IRequest<AuthenticateUserResult>
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthenticateUserResult
{
    public string Token { get; set; } = string.Empty;
}

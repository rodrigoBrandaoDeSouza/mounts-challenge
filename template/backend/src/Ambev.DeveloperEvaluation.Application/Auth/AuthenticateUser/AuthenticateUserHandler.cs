using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;

public class AuthenticateUserHandler : IRequestHandler<AuthenticateUserCommand, AuthenticateUserResult>
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthenticateUserHandler(IUserRepository repository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthenticateUserResult> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.Password))
            throw new InvalidCredentialsException("Invalid username or password");

        if (!user.IsActive)
            throw new InvalidCredentialsException($"The user is {user.Status.ToString().ToLowerInvariant()} and cannot sign in");

        return new AuthenticateUserResult { Token = _jwtTokenGenerator.GenerateToken(user) };
    }
}

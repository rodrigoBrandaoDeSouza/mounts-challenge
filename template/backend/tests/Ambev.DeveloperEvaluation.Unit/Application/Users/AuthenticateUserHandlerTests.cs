using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.TestData;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Users;

public class AuthenticateUserHandlerTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly AuthenticateUserHandler _handler;

    public AuthenticateUserHandlerTests()
    {
        _handler = new AuthenticateUserHandler(_repository, _passwordHasher, _tokenGenerator);
        _tokenGenerator.GenerateToken(Arg.Any<IUser>()).Returns("jwt-token");
    }

    [Fact(DisplayName = "Valid credentials return a token")]
    public async Task ValidCredentials_ReturnToken()
    {
        var user = UserTestData.User();
        _repository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword("secret", user.Password).Returns(true);

        var result = await _handler.Handle(new AuthenticateUserCommand { Username = user.Username, Password = "secret" }, CancellationToken.None);

        result.Token.Should().Be("jwt-token");
    }

    [Fact(DisplayName = "Unknown user is rejected")]
    public async Task UnknownUser_Throws()
    {
        _repository.GetByUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => _handler.Handle(new AuthenticateUserCommand { Username = "nobody", Password = "secret" }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact(DisplayName = "Wrong password is rejected")]
    public async Task WrongPassword_Throws()
    {
        var user = UserTestData.User();
        _repository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var act = () => _handler.Handle(new AuthenticateUserCommand { Username = user.Username, Password = "wrong" }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Theory(DisplayName = "Inactive or suspended users are rejected")]
    [InlineData(UserStatus.Inactive)]
    [InlineData(UserStatus.Suspended)]
    public async Task NotActiveUser_Throws(UserStatus status)
    {
        var user = UserTestData.User(status);
        _repository.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var act = () => _handler.Handle(new AuthenticateUserCommand { Username = user.Username, Password = "secret" }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }
}

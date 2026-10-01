using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Application.Users.UpdateUser;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Users;

public class UserHandlersTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IMapper _mapper = TestMapper.Create();

    public UserHandlersTests()
    {
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("hashed");
        _repository.CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var user = call.Arg<User>();
                user.Id = 1;
                return user;
            });
        _repository.UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<User>());
    }

    [Fact(DisplayName = "Create hashes the password and returns the user without it")]
    public async Task Create_HashesPassword()
    {
        var command = UserTestData.ValidCreateCommand();

        var result = await new CreateUserHandler(_repository, _passwordHasher, _mapper).Handle(command, CancellationToken.None);

        _passwordHasher.Received(1).HashPassword(command.Password);
        await _repository.Received(1).CreateAsync(Arg.Is<User>(u => u.Password == "hashed"), Arg.Any<CancellationToken>());
        result.Id.Should().Be(1);
        result.Email.Should().Be(command.Email);
        result.Name.Firstname.Should().Be(command.Name.Firstname);
        result.Address.Geolocation.Lat.Should().Be(command.Address.Geolocation.Lat);
        result.Status.Should().Be(UserStatus.Active);
        result.Role.Should().Be(UserRole.Customer);
    }

    [Fact(DisplayName = "Status and role default to Active and Customer")]
    public async Task Create_DefaultsStatusAndRole()
    {
        var command = UserTestData.ValidCreateCommand();
        command.Status = null;
        command.Role = null;

        var result = await new CreateUserHandler(_repository, _passwordHasher, _mapper).Handle(command, CancellationToken.None);

        result.Status.Should().Be(UserStatus.Active);
        result.Role.Should().Be(UserRole.Customer);
    }

    [Fact(DisplayName = "Create with an email already in use throws ConflictException")]
    public async Task Create_DuplicatedEmail_Throws()
    {
        var command = UserTestData.ValidCreateCommand();
        _repository.EmailExistsAsync(command.Email, null, Arg.Any<CancellationToken>()).Returns(true);

        var act = () => new CreateUserHandler(_repository, _passwordHasher, _mapper).Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact(DisplayName = "Update keeps the password when it is omitted")]
    public async Task Update_WithoutPassword_KeepsPassword()
    {
        var user = UserTestData.User();
        _repository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var create = UserTestData.ValidCreateCommand();
        var command = new UpdateUserCommand
        {
            Id = user.Id,
            Email = create.Email,
            Username = create.Username,
            Password = string.Empty,
            Name = create.Name,
            Address = create.Address,
            Phone = create.Phone,
            Role = UserRole.Admin
        };

        var result = await new UpdateUserHandler(_repository, _passwordHasher, _mapper).Handle(command, CancellationToken.None);

        user.Password.Should().Be("hashed-password");
        result.Email.Should().Be(create.Email);
        result.Role.Should().Be(UserRole.Admin);
        result.Status.Should().Be(UserStatus.Active);
    }

    [Fact(DisplayName = "Delete returns the deleted user")]
    public async Task Delete_ReturnsDeletedUser()
    {
        var user = UserTestData.User();
        _repository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _repository.DeleteAsync(user.Id, Arg.Any<CancellationToken>()).Returns(true);

        var result = await new DeleteUserHandler(_repository, _mapper).Handle(new DeleteUserCommand(user.Id), CancellationToken.None);

        result.Id.Should().Be(user.Id);
        await _repository.Received(1).DeleteAsync(user.Id, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "User validator accepts a valid user")]
    public void Validator_Valid()
    {
        new CreateUserValidator().Validate(UserTestData.ValidCreateCommand()).IsValid.Should().BeTrue();
    }

    [Theory(DisplayName = "User validator rejects weak passwords")]
    [InlineData("short1!")]
    [InlineData("alllowercase1!")]
    [InlineData("ALLUPPERCASE1!")]
    [InlineData("NoNumbers!!")]
    [InlineData("NoSpecial123")]
    public void Validator_WeakPassword(string password)
    {
        var command = UserTestData.ValidCreateCommand();
        command.Password = password;

        new CreateUserValidator().Validate(command).IsValid.Should().BeFalse();
    }

    [Fact(DisplayName = "User validator rejects invalid email and phone")]
    public void Validator_InvalidEmailAndPhone()
    {
        var command = UserTestData.ValidCreateCommand();
        command.Email = "invalid";
        command.Phone = "abc";

        var result = new CreateUserValidator().Validate(command);

        result.Errors.Select(e => e.PropertyName).Should().Contain(new[] { "Email", "Phone" });
    }

    [Fact(DisplayName = "Update validator allows an empty password")]
    public void UpdateValidator_EmptyPassword()
    {
        var create = UserTestData.ValidCreateCommand();
        var command = new UpdateUserCommand
        {
            Id = 1,
            Email = create.Email,
            Username = create.Username,
            Name = create.Name,
            Address = create.Address,
            Phone = create.Phone
        };

        new UpdateUserValidator().Validate(command).IsValid.Should().BeTrue();
    }
}

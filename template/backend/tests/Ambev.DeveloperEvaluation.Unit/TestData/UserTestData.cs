using Ambev.DeveloperEvaluation.Application.Users;
using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.TestData;

/// <summary>
/// Bogus fakers for the user tests.
/// </summary>
public static class UserTestData
{
    public const string ValidPassword = "Test@1234";

    public static CreateUserCommand ValidCreateCommand()
    {
        var faker = new Faker();
        return new CreateUserCommand
        {
            Email = faker.Internet.Email(),
            Username = faker.Internet.UserName().Replace(".", string.Empty) + faker.Random.Int(100, 999),
            Password = ValidPassword,
            Name = new UserNameDto { Firstname = faker.Name.FirstName(), Lastname = faker.Name.LastName() },
            Address = new UserAddressDto
            {
                City = faker.Address.City(),
                Street = faker.Address.StreetName(),
                Number = faker.Random.Int(1, 9999),
                Zipcode = "12345-678",
                Geolocation = new GeolocationDto { Lat = "-24.7136", Long = "-53.7405" }
            },
            Phone = "+55 45 99999-0000",
            Status = UserStatus.Active,
            Role = UserRole.Customer
        };
    }

    public static User User(UserStatus status = UserStatus.Active)
    {
        var faker = new Faker();
        return new User
        {
            Id = faker.Random.Int(1, 1000),
            Email = faker.Internet.Email(),
            Username = faker.Internet.UserName(),
            Password = "hashed-password",
            Name = new PersonName { Firstname = faker.Name.FirstName(), Lastname = faker.Name.LastName() },
            Address = new Address { City = faker.Address.City(), Street = faker.Address.StreetName(), Number = 10, Zipcode = "12345-678" },
            Phone = "+55 45 99999-0000",
            Status = status,
            Role = UserRole.Customer
        };
    }
}

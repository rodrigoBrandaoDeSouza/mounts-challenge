using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

public class UserAndCartRepositoryTests
{
    private readonly ContextFactory _factory = new();

    private static User NewUser(string username, string email) => new()
    {
        Username = username,
        Email = email,
        Password = "hash",
        Phone = "+55 45 99999-0000",
        Name = new PersonName { Firstname = "John", Lastname = "Doe" },
        Address = new Address { City = "Toledo", Street = "Rua A", Number = 1, Zipcode = "85900-000", Latitude = "-24.7", Longitude = "-53.7" },
        Status = UserStatus.Active,
        Role = UserRole.Customer
    };

    [Fact(DisplayName = "User lookups and uniqueness checks")]
    public async Task User_LookupsAndUniqueness()
    {
        int userId;
        await using (var context = _factory.Create())
        {
            var created = await new UserRepository(context).CreateAsync(NewUser("john", "john@example.com"));
            userId = created.Id;
        }

        await using (var context = _factory.Create())
        {
            var repository = new UserRepository(context);

            (await repository.GetByUsernameAsync("john"))!.Address.City.Should().Be("Toledo");
            (await repository.ExistsAsync(userId)).Should().BeTrue();
            (await repository.EmailExistsAsync("JOHN@example.com")).Should().BeTrue();
            (await repository.EmailExistsAsync("john@example.com", userId)).Should().BeFalse();
            (await repository.UsernameExistsAsync("john")).Should().BeTrue();
            (await repository.UsernameExistsAsync("mary")).Should().BeFalse();
        }
    }

    [Fact(DisplayName = "Cart products are replaced on update")]
    public async Task Cart_ReplaceProducts()
    {
        int cartId;
        await using (var context = _factory.Create())
        {
            var cart = new Cart
            {
                UserId = 1,
                Date = DateTime.UtcNow,
                Products = new List<CartProduct>
                {
                    new() { ProductId = 1, Quantity = 1 },
                    new() { ProductId = 2, Quantity = 2 }
                }
            };
            cartId = (await new CartRepository(context).CreateAsync(cart)).Id;
        }

        await using (var context = _factory.Create())
        {
            var repository = new CartRepository(context);
            var cart = (await repository.GetByIdAsync(cartId))!;
            cart.ReplaceProducts(new[] { new CartProduct { ProductId = 3, Quantity = 7 } });
            await repository.UpdateAsync(cart);
        }

        await using (var context = _factory.Create())
        {
            var cart = (await new CartRepository(context).GetByIdAsync(cartId))!;
            cart.Products.Should().ContainSingle().Which.ProductId.Should().Be(3);
        }
    }
}

using FluentAssertions;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

public class CatalogApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public CatalogApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    private static object NewProduct(string title, decimal price, string category) => new
    {
        title,
        price,
        description = "Your perfect pack for everyday use",
        category,
        image = "https://fakestoreapi.com/img/81fPKd-2AYL._AC_SL1500_.jpg",
        rating = new { rate = 3.9m, count = 120 }
    };

    [Fact(DisplayName = "Products CRUD, categories and carts")]
    public async Task ProductsAndCarts()
    {
        var client = _factory.CreateClient();
        var userId = await client.AuthenticateAsync();

        // Products
        var created = await client.PostJsonAsync("/api/products", NewProduct("Backpack", 109.95m, "men's clothing"));
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var productId = (await created.ReadAsync<JsonElement>()).GetProperty("id").GetInt32();
        (await client.PostJsonAsync("/api/products", NewProduct("Bracelet", 695m, "jewelery"))).EnsureSuccessStatusCode();

        var updated = await client.PutJsonAsync($"/api/products/{productId}", NewProduct("Backpack v2", 99.90m, "men's clothing"));
        updated.StatusCode.Should().Be(HttpStatusCode.OK);
        (await updated.ReadAsync<JsonElement>()).GetProperty("title").GetString().Should().Be("Backpack v2");

        var categories = await client.GetAsync("/api/products/categories");
        (await categories.ReadAsync<string[]>()).Should().Contain(new[] { "jewelery", "men's clothing" });

        var byCategory = await client.GetAsync("/api/products/category/jewelery");
        var page = await byCategory.ReadAsync<JsonElement>();
        page.GetProperty("data").EnumerateArray().Should().OnlyContain(p => p.GetProperty("category").GetString() == "jewelery");

        var filtered = await client.GetAsync("/api/products?_minPrice=50&_maxPrice=200&_order=price desc");
        (await filtered.ReadAsync<JsonElement>()).GetProperty("data").EnumerateArray()
            .Should().OnlyContain(p => p.GetProperty("price").GetDecimal() >= 50m && p.GetProperty("price").GetDecimal() <= 200m);

        // Carts
        var cart = await client.PostJsonAsync("/api/carts", new
        {
            userId,
            date = "2024-03-01T00:00:00Z",
            products = new[] { new { productId, quantity = 2 } }
        });
        cart.StatusCode.Should().Be(HttpStatusCode.Created);
        var cartId = (await cart.ReadAsync<JsonElement>()).GetProperty("id").GetInt32();

        var cartWithUnknownProduct = await client.PutJsonAsync($"/api/carts/{cartId}", new
        {
            userId,
            date = "2024-03-02T00:00:00Z",
            products = new[] { new { productId = 99999, quantity = 1 } }
        });
        cartWithUnknownProduct.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var carts = await client.GetAsync($"/api/carts?userId={userId}");
        (await carts.ReadAsync<JsonElement>()).GetProperty("totalItems").GetInt32().Should().Be(1);

        var deleteCart = await client.DeleteAsync($"/api/carts/{cartId}");
        (await deleteCart.ReadAsync<JsonElement>()).GetProperty("message").GetString().Should().NotBeNullOrWhiteSpace();

        var deleteProduct = await client.DeleteAsync($"/api/products/{productId}");
        deleteProduct.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Users: get, update, list and delete (returns the deleted user)")]
    public async Task Users()
    {
        var client = _factory.CreateClient();
        var userId = await client.AuthenticateAsync();

        var get = await client.GetAsync($"/api/users/{userId}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await get.ReadAsync<JsonElement>();
        user.TryGetProperty("password", out _).Should().BeFalse();
        user.GetProperty("role").GetString().Should().Be("Admin");

        var update = await client.PutJsonAsync($"/api/users/{userId}", new
        {
            email = user.GetProperty("email").GetString(),
            username = user.GetProperty("username").GetString(),
            name = new { firstname = "Jane", lastname = "Doe" },
            address = new { city = "Cascavel", street = "Rua B", number = 2, zipcode = "85800-000", geolocation = new { lat = "-24.9", @long = "-53.4" } },
            phone = "+55 45 98888-0000",
            status = "Active",
            role = "Manager"
        });
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        (await update.ReadAsync<JsonElement>()).GetProperty("name").GetProperty("firstname").GetString().Should().Be("Jane");

        var list = await client.GetAsync("/api/users?_order=username asc&role=Manager");
        (await list.ReadAsync<JsonElement>()).GetProperty("data").EnumerateArray()
            .Should().Contain(u => u.GetProperty("id").GetInt32() == userId);

        var delete = await client.DeleteAsync($"/api/users/{userId}");
        delete.StatusCode.Should().Be(HttpStatusCode.OK);
        (await delete.ReadAsync<JsonElement>()).GetProperty("id").GetInt32().Should().Be(userId);
    }
}

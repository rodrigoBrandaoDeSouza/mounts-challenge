using FluentAssertions;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

public class SalesApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public SalesApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    private static object NewSale(string saleNumber, int beerQuantity = 12) => new
    {
        saleNumber,
        date = "2025-10-20T14:30:00Z",
        customerId = Guid.NewGuid(),
        customerName = "Bar do Zé",
        branchId = Guid.NewGuid(),
        branchName = "Filial Toledo",
        items = new object[]
        {
            new { productId = Guid.NewGuid(), productName = "Cerveja 600ml", quantity = beerQuantity, unitPrice = 9.90m },
            new { productId = Guid.NewGuid(), productName = "Refrigerante 2L", quantity = 2, unitPrice = 11.50m }
        }
    };

    private static string NewSaleNumber() => $"S-{Guid.NewGuid():N}"[..20];

    [Fact(DisplayName = "Full sale lifecycle: create, get, update, cancel item, cancel, delete")]
    public async Task SaleLifecycle()
    {
        var client = _factory.CreateClient();
        await client.AuthenticateAsync();

        // Create
        var create = await client.PostJsonAsync("/api/sales", NewSale(NewSaleNumber()));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        create.Headers.Location.Should().NotBeNull();
        var created = await create.ReadAsync<JsonElement>();
        var id = created.GetProperty("id").GetGuid();
        created.GetProperty("totalAmount").GetDecimal().Should().Be(118.04m);
        created.GetProperty("cancelled").GetBoolean().Should().BeFalse();

        // Get
        var get = await client.GetAsync($"/api/sales/{id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var sale = await get.ReadAsync<JsonElement>();
        var items = sale.GetProperty("items").EnumerateArray().ToList();
        items.Should().HaveCount(2);
        var beer = items.Single(i => i.GetProperty("productName").GetString() == "Cerveja 600ml");
        beer.GetProperty("discountPercent").GetDecimal().Should().Be(20m);

        // Update: keep the beer with 5 units (10%), drop the soda, add water
        var update = await client.PutJsonAsync($"/api/sales/{id}", new
        {
            saleNumber = sale.GetProperty("saleNumber").GetString(),
            customerId = sale.GetProperty("customerId").GetGuid(),
            customerName = "Bar do Zé",
            branchId = sale.GetProperty("branchId").GetGuid(),
            branchName = "Filial Toledo",
            items = new object[]
            {
                new
                {
                    id = beer.GetProperty("id").GetGuid(),
                    productId = beer.GetProperty("productId").GetGuid(),
                    productName = "Cerveja 600ml",
                    quantity = 5,
                    unitPrice = 10m
                },
                new { productId = Guid.NewGuid(), productName = "Água 500ml", quantity = 1, unitPrice = 3m }
            }
        });
        update.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await update.ReadAsync<JsonElement>();
        updated.GetProperty("items").GetArrayLength().Should().Be(2);
        updated.GetProperty("totalAmount").GetDecimal().Should().Be(48m);
        updated.GetProperty("date").GetDateTime().Should().Be(sale.GetProperty("date").GetDateTime());

        // Cancel item
        var waterId = updated.GetProperty("items").EnumerateArray()
            .Single(i => i.GetProperty("productName").GetString() == "Água 500ml")
            .GetProperty("id").GetGuid();
        var cancelItem = await client.PatchAsync($"/api/sales/{id}/items/{waterId}/cancel", null);
        cancelItem.StatusCode.Should().Be(HttpStatusCode.OK);
        (await cancelItem.ReadAsync<JsonElement>()).GetProperty("totalAmount").GetDecimal().Should().Be(45m);

        // Cancel sale
        var cancel = await client.PatchAsync($"/api/sales/{id}/cancel", null);
        cancel.StatusCode.Should().Be(HttpStatusCode.OK);
        (await cancel.ReadAsync<JsonElement>()).GetProperty("cancelled").GetBoolean().Should().BeTrue();

        // A cancelled sale cannot be cancelled again
        var cancelAgain = await client.PatchAsync($"/api/sales/{id}/cancel", null);
        cancelAgain.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await cancelAgain.ReadAsync<JsonElement>()).GetProperty("type").GetString().Should().Be("BusinessRuleViolation");

        // Delete
        var delete = await client.DeleteAsync($"/api/sales/{id}");
        delete.StatusCode.Should().Be(HttpStatusCode.OK);
        (await delete.ReadAsync<JsonElement>()).GetProperty("message").GetString().Should().NotBeNullOrWhiteSpace();

        var getDeleted = await client.GetAsync($"/api/sales/{id}");
        getDeleted.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "More than 20 identical items is rejected")]
    public async Task AboveLimit_Returns400()
    {
        var client = _factory.CreateClient();
        await client.AuthenticateAsync();

        var response = await client.PostJsonAsync("/api/sales", NewSale(NewSaleNumber(), beerQuantity: 21));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ReadAsync<JsonElement>()).GetProperty("type").GetString().Should().Be("ValidationError");
    }

    [Fact(DisplayName = "Duplicated sale number returns 409")]
    public async Task DuplicatedSaleNumber_Returns409()
    {
        var client = _factory.CreateClient();
        await client.AuthenticateAsync();
        var saleNumber = NewSaleNumber();

        (await client.PostJsonAsync("/api/sales", NewSale(saleNumber))).StatusCode.Should().Be(HttpStatusCode.Created);
        var duplicated = await client.PostJsonAsync("/api/sales", NewSale(saleNumber));

        duplicated.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "List returns the paginated envelope")]
    public async Task List_ReturnsPaginatedEnvelope()
    {
        var client = _factory.CreateClient();
        await client.AuthenticateAsync();
        for (var i = 0; i < 3; i++)
            (await client.PostJsonAsync("/api/sales", NewSale(NewSaleNumber()))).EnsureSuccessStatusCode();

        var response = await client.GetAsync("/api/sales?_page=1&_size=2&_order=saleNumber asc&customerName=Bar*");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.ReadAsync<JsonElement>();
        page.GetProperty("data").GetArrayLength().Should().Be(2);
        page.GetProperty("totalItems").GetInt32().Should().BeGreaterThanOrEqualTo(3);
        page.GetProperty("currentPage").GetInt32().Should().Be(1);
        page.GetProperty("totalPages").GetInt32().Should().BeGreaterThanOrEqualTo(2);
    }
}

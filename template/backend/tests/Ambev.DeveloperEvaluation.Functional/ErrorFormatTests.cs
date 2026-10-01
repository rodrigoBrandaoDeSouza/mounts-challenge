using FluentAssertions;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

public class ErrorFormatTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public ErrorFormatTests(ApiFactory factory)
    {
        _factory = factory;
    }

    private static async Task AssertErrorAsync(HttpResponseMessage response, HttpStatusCode status, string type)
    {
        response.StatusCode.Should().Be(status);
        var error = await response.ReadAsync<JsonElement>();
        error.GetProperty("type").GetString().Should().Be(type);
        error.GetProperty("error").GetString().Should().NotBeNullOrWhiteSpace();
        error.GetProperty("detail").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "Missing token returns 401 AuthenticationError")]
    public async Task MissingToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/sales");

        await AssertErrorAsync(response, HttpStatusCode.Unauthorized, "AuthenticationError");
    }

    [Fact(DisplayName = "Invalid credentials return 401 AuthenticationError")]
    public async Task InvalidCredentials_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostJsonAsync("/api/auth/login", new { username = "nobody", password = "Wrong@123" });

        await AssertErrorAsync(response, HttpStatusCode.Unauthorized, "AuthenticationError");
    }

    [Fact(DisplayName = "Unknown resource returns 404 ResourceNotFound")]
    public async Task UnknownResource_Returns404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/products/12345");

        await AssertErrorAsync(response, HttpStatusCode.NotFound, "ResourceNotFound");
    }

    [Fact(DisplayName = "Invalid body returns 400 ValidationError")]
    public async Task InvalidBody_Returns400()
    {
        var client = _factory.CreateClient();
        await client.AuthenticateAsync();

        var response = await client.PostJsonAsync("/api/products", new { title = "", price = -1, category = "" });

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, "ValidationError");
    }

    [Fact(DisplayName = "Malformed JSON returns 400 ValidationError")]
    public async Task MalformedJson_Returns400()
    {
        var client = _factory.CreateClient();
        await client.AuthenticateAsync();

        var response = await client.PostAsync("/api/products", new StringContent("{ not json", Encoding.UTF8, "application/json"));

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, "ValidationError");
    }

    [Fact(DisplayName = "Invalid list parameters return 400 ValidationError")]
    public async Task InvalidListParameters_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/products?_order=unknown desc");

        await AssertErrorAsync(response, HttpStatusCode.BadRequest, "ValidationError");
    }
}

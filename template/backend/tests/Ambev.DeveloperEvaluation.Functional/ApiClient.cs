using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ambev.DeveloperEvaluation.Functional;

/// <summary>
/// Helpers to call the API in the tests.
/// </summary>
public static class ApiClient
{
    public static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;

    public static Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string url, T body) =>
        client.PostAsJsonAsync(url, body, JsonOptions);

    public static Task<HttpResponseMessage> PutJsonAsync<T>(this HttpClient client, string url, T body) =>
        client.PutAsJsonAsync(url, body, JsonOptions);

    /// <summary>
    /// Registers a new user, signs in and sets the bearer token on the client.
    /// </summary>
    public static async Task<int> AuthenticateAsync(this HttpClient client)
    {
        var username = $"user{Guid.NewGuid():N}"[..20];
        const string password = "Test@1234";

        var register = await client.PostJsonAsync("/api/users", new
        {
            email = $"{username}@example.com",
            username,
            password,
            name = new { firstname = "John", lastname = "Doe" },
            address = new
            {
                city = "Toledo",
                street = "Rua Barão do Rio Branco",
                number = 100,
                zipcode = "85900-000",
                geolocation = new { lat = "-24.7136", @long = "-53.7405" }
            },
            phone = "+55 45 99999-0000",
            status = "Active",
            role = "Admin"
        });
        register.EnsureSuccessStatusCode();
        var user = await register.ReadAsync<JsonElement>();

        var login = await client.PostJsonAsync("/api/auth/login", new { username, password });
        login.EnsureSuccessStatusCode();
        var token = (await login.ReadAsync<JsonElement>()).GetProperty("token").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return user.GetProperty("id").GetInt32();
    }
}

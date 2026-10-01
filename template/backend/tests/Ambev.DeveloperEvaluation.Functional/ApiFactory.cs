using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ambev.DeveloperEvaluation.Functional;

/// <summary>
/// Hosts the real API in memory, replacing PostgreSQL by an EF Core in-memory database.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"functional-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<DefaultContext>>();
            services.RemoveAll<DbContextOptions>();
            services.AddDbContext<DefaultContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}

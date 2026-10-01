using Ambev.DeveloperEvaluation.ORM;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.Integration;

/// <summary>
/// Creates <see cref="DefaultContext"/> instances over an isolated EF Core in-memory database.
/// </summary>
public sealed class ContextFactory
{
    private readonly DbContextOptions<DefaultContext> _options = new DbContextOptionsBuilder<DefaultContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

    public DefaultContext Create() => new(_options);
}

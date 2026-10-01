using Ambev.DeveloperEvaluation.Application.Common.Querying;
using Ambev.DeveloperEvaluation.Application.Products;
using Ambev.DeveloperEvaluation.Application.Products.ListProducts;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

public class ProductRepositoryTests
{
    private readonly ContextFactory _factory = new();

    private async Task SeedAsync()
    {
        await using var context = _factory.Create();
        var repository = new ProductRepository(context);
        await repository.CreateAsync(new Product { Title = "Fjallraven - Foldsack No. 1 Backpack", Price = 109.95m, Category = "men's clothing", Rating = new Rating { Rate = 3.9m, Count = 120 } });
        await repository.CreateAsync(new Product { Title = "Mens Casual Premium Slim Fit T-Shirts", Price = 22.30m, Category = "men's clothing", Rating = new Rating { Rate = 4.1m, Count = 259 } });
        await repository.CreateAsync(new Product { Title = "John Hardy Women's Bracelet", Price = 695m, Category = "jewelery", Rating = new Rating { Rate = 4.6m, Count = 400 } });
        await repository.CreateAsync(new Product { Title = "WD 2TB Elements Portable Hard Drive", Price = 64m, Category = "electronics", Rating = new Rating { Rate = 3.3m, Count = 203 } });
    }

    private async Task<IReadOnlyList<Product>> ListAsync(ListProductsQuery query)
    {
        await using var context = _factory.Create();
        var page = await new ProductRepository(context).ListAsync(QueryOptionsParser.Parse(query, ProductQueryFields.Map));
        return page.Items;
    }

    [Fact(DisplayName = "Categories are distinct and ordered")]
    public async Task GetCategories()
    {
        await SeedAsync();
        await using var context = _factory.Create();

        var categories = await new ProductRepository(context).GetCategoriesAsync();

        categories.Should().Equal("electronics", "jewelery", "men's clothing");
    }

    [Fact(DisplayName = "Wildcard filters do partial, case-insensitive matches")]
    public async Task List_Wildcards()
    {
        await SeedAsync();

        var startsWith = new ListProductsQuery();
        startsWith.Filters["title"] = new[] { "fjallraven*" };
        (await ListAsync(startsWith)).Should().ContainSingle();

        var endsWith = new ListProductsQuery();
        endsWith.Filters["category"] = new[] { "*clothing" };
        (await ListAsync(endsWith)).Should().HaveCount(2);
    }

    [Fact(DisplayName = "Same field informed twice behaves as OR")]
    public async Task List_SameFieldTwice()
    {
        await SeedAsync();

        var query = new ListProductsQuery();
        query.Filters["category"] = new[] { "jewelery", "electronics" };

        (await ListAsync(query)).Should().HaveCount(2);
    }

    [Fact(DisplayName = "Price range and ordering by nested field")]
    public async Task List_RangeAndNestedOrdering()
    {
        await SeedAsync();

        var query = new ListProductsQuery { Order = "rating.rate desc" };
        query.Filters["_minPrice"] = new[] { "50" };
        query.Filters["_maxPrice"] = new[] { "700" };

        (await ListAsync(query)).Select(p => p.Category).Should().Equal("jewelery", "men's clothing", "electronics");
    }

    [Fact(DisplayName = "GetExistingIds returns only the existing identifiers")]
    public async Task GetExistingIds()
    {
        await SeedAsync();
        await using var context = _factory.Create();

        var ids = await new ProductRepository(context).GetExistingIdsAsync(new[] { 1, 2, 999 });

        ids.Should().BeEquivalentTo(new[] { 1, 2 });
    }
}

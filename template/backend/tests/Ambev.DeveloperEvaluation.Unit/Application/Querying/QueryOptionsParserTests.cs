using Ambev.DeveloperEvaluation.Application.Products;
using Ambev.DeveloperEvaluation.Application.Products.ListProducts;
using Ambev.DeveloperEvaluation.Application.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Common;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Querying;

public class QueryOptionsParserTests
{
    private static ListProductsQuery Query(string? order = null, int? page = null, int? size = null, (string Key, string[] Values)[]? filters = null)
    {
        var query = new ListProductsQuery { Order = order, Page = page, Size = size };
        foreach (var (key, values) in filters ?? Array.Empty<(string, string[])>())
            query.Filters[key] = values;
        return query;
    }

    [Fact(DisplayName = "Defaults: page 1, size 10 and the default ordering")]
    public void Parse_Defaults()
    {
        var options = QueryOptionsParser.Parse(Query(), ProductQueryFields.Map);

        options.Page.Should().Be(1);
        options.Size.Should().Be(10);
        options.Orderings.Should().ContainSingle().Which.Should().Be(new OrderCriterion("Id", false));
        options.FilterGroups.Should().BeEmpty();
    }

    [Fact(DisplayName = "_order accepts several fields, quotes and optional direction")]
    public void Parse_Order()
    {
        var options = QueryOptionsParser.Parse(Query("\"price desc, title\""), ProductQueryFields.Map);

        options.Orderings.Should().Equal(
            new OrderCriterion("Price", true),
            new OrderCriterion("Title", false));
    }

    [Fact(DisplayName = "_order with nested fields uses the property path")]
    public void Parse_OrderNestedField()
    {
        var options = QueryOptionsParser.Parse(Query("rating.rate desc"), ProductQueryFields.Map);

        options.Orderings.Should().ContainSingle().Which.Should().Be(new OrderCriterion("Rating.Rate", true));
    }

    [Theory(DisplayName = "String filters support wildcards")]
    [InlineData("Fjallraven*", FilterOperator.StartsWith, "Fjallraven")]
    [InlineData("*clothing", FilterOperator.EndsWith, "clothing")]
    [InlineData("*pack*", FilterOperator.Contains, "pack")]
    [InlineData("men's clothing", FilterOperator.Equal, "men's clothing")]
    public void Parse_StringFilters(string value, FilterOperator expectedOperator, string expectedValue)
    {
        var options = QueryOptionsParser.Parse(Query(filters: new[] { ("title", new[] { value }) }), ProductQueryFields.Map);

        options.FilterGroups.Should().ContainSingle()
            .Which.Should().ContainSingle()
            .Which.Should().Be(new FilterCriterion("Title", expectedOperator, expectedValue));
    }

    [Fact(DisplayName = "_min and _max create range filters with typed values")]
    public void Parse_RangeFilters()
    {
        var options = QueryOptionsParser.Parse(
            Query(filters: new[] { ("_minPrice", new[] { "50" }), ("_maxPrice", new[] { "200.5" }) }),
            ProductQueryFields.Map);

        options.FilterGroups.SelectMany(g => g).Should().BeEquivalentTo(new[]
        {
            new FilterCriterion("Price", FilterOperator.GreaterThanOrEqual, 50m),
            new FilterCriterion("Price", FilterOperator.LessThanOrEqual, 200.5m)
        });
    }

    [Fact(DisplayName = "The same field informed twice is combined with OR (same group)")]
    public void Parse_SameFieldTwice_SameGroup()
    {
        var options = QueryOptionsParser.Parse(
            Query(filters: new[] { ("category", new[] { "electronics", "jewelery" }) }),
            ProductQueryFields.Map);

        options.FilterGroups.Should().ContainSingle().Which.Should().HaveCount(2);
    }

    [Theory(DisplayName = "Invalid parameters throw ValidationException")]
    [InlineData("unknown asc", null, null)]
    [InlineData("price sideways", null, null)]
    [InlineData(null, 0, null)]
    [InlineData(null, null, 0)]
    [InlineData(null, null, 101)]
    public void Parse_InvalidParameters_Throws(string? order, int? page, int? size)
    {
        var act = () => QueryOptionsParser.Parse(Query(order, page, size), ProductQueryFields.Map);

        act.Should().Throw<ValidationException>();
    }

    [Theory(DisplayName = "Invalid filters throw ValidationException")]
    [InlineData("unknownField", "x")]
    [InlineData("price", "abc")]
    [InlineData("_minTitle", "a")]
    [InlineData("_minPrice", "abc")]
    public void Parse_InvalidFilters_Throws(string key, string value)
    {
        var act = () => QueryOptionsParser.Parse(Query(filters: new[] { (key, new[] { value }) }), ProductQueryFields.Map);

        act.Should().Throw<ValidationException>();
    }
}

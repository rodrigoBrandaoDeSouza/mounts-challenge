using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.ListCategories;

/// <summary>
/// Retrieves all product categories.
/// </summary>
public record ListCategoriesQuery : IRequest<IReadOnlyList<string>>;

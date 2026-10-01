using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.ListCategories;

public class ListCategoriesHandler : IRequestHandler<ListCategoriesQuery, IReadOnlyList<string>>
{
    private readonly IProductRepository _repository;

    public ListCategoriesHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<string>> Handle(ListCategoriesQuery request, CancellationToken cancellationToken) =>
        _repository.GetCategoriesAsync(cancellationToken);
}

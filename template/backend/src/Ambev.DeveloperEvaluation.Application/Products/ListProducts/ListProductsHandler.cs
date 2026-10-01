using Ambev.DeveloperEvaluation.Application.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.ListProducts;

public class ListProductsHandler : IRequestHandler<ListProductsQuery, PagedResult<ProductResult>>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public ListProductsHandler(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductResult>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var options = QueryOptionsParser.Parse(request, ProductQueryFields.Map);
        var products = await _repository.ListAsync(options, cancellationToken);
        return products.Map(p => _mapper.Map<ProductResult>(p));
    }
}

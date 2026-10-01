using Ambev.DeveloperEvaluation.Application.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.ListCarts;

public class ListCartsHandler : IRequestHandler<ListCartsQuery, PagedResult<CartResult>>
{
    private readonly ICartRepository _repository;
    private readonly IMapper _mapper;

    public ListCartsHandler(ICartRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<CartResult>> Handle(ListCartsQuery request, CancellationToken cancellationToken)
    {
        var options = QueryOptionsParser.Parse(request, CartQueryFields.Map);
        var carts = await _repository.ListAsync(options, cancellationToken);
        return carts.Map(c => _mapper.Map<CartResult>(c));
    }
}

using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Products.UpdateProduct;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ProductResult>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public UpdateProductHandler(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ProductResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw ResourceNotFoundException.For("Product", request.Id);

        _mapper.Map(request, product);

        var updated = await _repository.UpdateAsync(product, cancellationToken);
        return _mapper.Map<ProductResult>(updated);
    }
}

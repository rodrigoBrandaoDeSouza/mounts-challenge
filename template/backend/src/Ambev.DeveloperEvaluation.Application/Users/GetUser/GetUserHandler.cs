using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.GetUser;

public class GetUserHandler : IRequestHandler<GetUserQuery, UserResult>
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    public GetUserHandler(IUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserResult> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw ResourceNotFoundException.For("User", request.Id);

        return _mapper.Map<UserResult>(user);
    }
}

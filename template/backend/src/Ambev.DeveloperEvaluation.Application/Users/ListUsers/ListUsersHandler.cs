using Ambev.DeveloperEvaluation.Application.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.ListUsers;

public class ListUsersHandler : IRequestHandler<ListUsersQuery, PagedResult<UserResult>>
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    public ListUsersHandler(IUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<UserResult>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var options = QueryOptionsParser.Parse(request, UserQueryFields.Map);
        var users = await _repository.ListAsync(options, cancellationToken);
        return users.Map(u => _mapper.Map<UserResult>(u));
    }
}

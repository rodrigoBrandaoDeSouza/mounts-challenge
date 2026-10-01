using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.DeleteUser;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, UserResult>
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    public DeleteUserHandler(IUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw ResourceNotFoundException.For("User", request.Id);

        var result = _mapper.Map<UserResult>(user);
        await _repository.DeleteAsync(request.Id, cancellationToken);
        return result;
    }
}

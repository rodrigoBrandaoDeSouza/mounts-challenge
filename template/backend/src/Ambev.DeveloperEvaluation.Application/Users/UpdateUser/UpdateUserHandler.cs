using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.UpdateUser;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UserResult>
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public UpdateUserHandler(IUserRepository repository, IPasswordHasher passwordHasher, IMapper mapper)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<UserResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw ResourceNotFoundException.For("User", request.Id);

        await UserUniqueness.EnsureAsync(_repository, request.Email, request.Username, request.Id, cancellationToken);

        _mapper.Map(request, user);

        if (!string.IsNullOrEmpty(request.Password))
            user.Password = _passwordHasher.HashPassword(request.Password);

        user.Status = request.Status ?? user.Status;
        user.Role = request.Role ?? user.Role;

        var updated = await _repository.UpdateAsync(user, cancellationToken);
        return _mapper.Map<UserResult>(updated);
    }
}

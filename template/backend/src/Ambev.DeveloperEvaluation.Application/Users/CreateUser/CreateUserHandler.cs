using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.CreateUser;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserResult>
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public CreateUserHandler(IUserRepository repository, IPasswordHasher passwordHasher, IMapper mapper)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<UserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        await UserUniqueness.EnsureAsync(_repository, request.Email, request.Username, null, cancellationToken);

        var user = _mapper.Map<User>(request);
        user.Password = _passwordHasher.HashPassword(request.Password);
        user.Status = request.Status ?? UserStatus.Active;
        user.Role = request.Role ?? UserRole.Customer;

        var created = await _repository.CreateAsync(user, cancellationToken);
        return _mapper.Map<UserResult>(created);
    }
}

using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Users;

internal static class UserUniqueness
{
    /// <summary>
    /// Ensures that no other user already uses the email or the username.
    /// </summary>
    public static async Task EnsureAsync(IUserRepository repository, string email, string username, int? userId, CancellationToken cancellationToken)
    {
        if (await repository.EmailExistsAsync(email, userId, cancellationToken))
            throw new ConflictException("Email already in use", $"There is already a user with the email '{email}'");

        if (await repository.UsernameExistsAsync(username, userId, cancellationToken))
            throw new ConflictException("Username already in use", $"There is already a user with the username '{username}'");
    }
}

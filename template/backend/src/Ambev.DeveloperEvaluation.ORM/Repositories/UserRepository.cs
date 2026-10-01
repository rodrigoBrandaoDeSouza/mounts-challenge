using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Querying;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUserRepository"/>.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly DefaultContext _context;

    public UserRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public Task<PagedResult<User>> ListAsync(QueryOptions options, CancellationToken cancellationToken = default) =>
        _context.Users
            .AsNoTracking()
            .ToPagedResultAsync(options, cancellationToken);

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        if (_context.Entry(user).State == EntityState.Detached)
            _context.Users.Update(user);

        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user is null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Users.AnyAsync(u => u.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, int? ignoreUserId = null, CancellationToken cancellationToken = default)
    {
        var normalized = email.ToLower();
        return _context.Users.AnyAsync(
            u => u.Email.ToLower() == normalized && (ignoreUserId == null || u.Id != ignoreUserId),
            cancellationToken);
    }

    public Task<bool> UsernameExistsAsync(string username, int? ignoreUserId = null, CancellationToken cancellationToken = default) =>
        _context.Users.AnyAsync(
            u => u.Username == username && (ignoreUserId == null || u.Id != ignoreUserId),
            cancellationToken);
}

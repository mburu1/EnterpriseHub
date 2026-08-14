using EnterpriseHub.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseHub.Infrastructure.Persistence.Mssql.Repositories;

public sealed class UserRepository(MssqlDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = Email.Create(email);
        return dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalized, ct);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = Email.Create(email);
        return dbContext.Users.AnyAsync(u => u.Email == normalized, ct);
    }

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        dbContext.Users.Add(user);
        return Task.CompletedTask;
    }

    public void Update(User user) => dbContext.Users.Update(user);
}

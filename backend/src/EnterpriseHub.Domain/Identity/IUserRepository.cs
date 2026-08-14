namespace EnterpriseHub.Domain.Identity;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, Guid tenantId, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, Guid tenantId, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
}

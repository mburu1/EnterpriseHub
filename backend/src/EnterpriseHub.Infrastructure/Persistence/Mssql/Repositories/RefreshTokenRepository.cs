using EnterpriseHub.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseHub.Infrastructure.Persistence.Mssql.Repositories;

public sealed class RefreshTokenRepository(MssqlDbContext dbContext) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default) =>
        dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);

    public Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        dbContext.RefreshTokens.Add(token);
        return Task.CompletedTask;
    }

    public void Update(RefreshToken token) => dbContext.RefreshTokens.Update(token);
}

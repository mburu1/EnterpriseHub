using EnterpriseHub.Domain.Identity;
using EnterpriseHub.Infrastructure.Persistence.Mssql;
using EnterpriseHub.Infrastructure.Persistence.Mssql.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Xunit;

namespace EnterpriseHub.Tests.Integration.Persistence;

/// <summary>Exercises UserRepository against a real SQL Server instance spun up via Testcontainers,
/// so the EF Core mapping (Email conversion, unique index, RefreshTokens navigation) is verified
/// against the actual provider rather than an in-memory substitute.</summary>
public sealed class UserRepositoryTests : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    private MssqlDbContext _dbContext = null!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<MssqlDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;

        _dbContext = new MssqlDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_ThenGetByEmail_RoundTripsTheUser()
    {
        var repository = new UserRepository(_dbContext);
        var user = User.Register(Guid.NewGuid(), Email.Create("owner@acme.com"), "hashed-password", "Ada", "Lovelace", TenantRole.Owner);

        await repository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var fetched = await repository.GetByEmailAsync("owner@acme.com");

        Assert.NotNull(fetched);
        Assert.Equal(user.Id, fetched!.Id);
        Assert.Equal("Ada", fetched.FirstName);
    }

    [Fact]
    public async Task ExistsByEmailAsync_IsCaseInsensitive()
    {
        var repository = new UserRepository(_dbContext);
        var user = User.Register(Guid.NewGuid(), Email.Create("owner@acme.com"), "hashed-password", "Ada", "Lovelace", TenantRole.Owner);
        await repository.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var exists = await repository.ExistsByEmailAsync("OWNER@ACME.COM");

        Assert.True(exists);
    }

    [Fact]
    public async Task IssueRefreshToken_PersistsAlongsideUser()
    {
        var repository = new UserRepository(_dbContext);
        var user = User.Register(Guid.NewGuid(), Email.Create("owner@acme.com"), "hashed-password", "Ada", "Lovelace", TenantRole.Owner);
        user.IssueRefreshToken("token-hash", TimeSpan.FromDays(7));

        await repository.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var fetched = await _dbContext.Users.Include(u => u.RefreshTokens).FirstAsync(u => u.Id == user.Id);

        Assert.Single(fetched.RefreshTokens);
    }
}

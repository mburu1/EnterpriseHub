using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Infrastructure.Cache;
using EnterpriseHub.Infrastructure.Persistence.Mssql;
using EnterpriseHub.Tests.E2E.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;
using Xunit;

namespace EnterpriseHub.Tests.E2E.Auth;

/// <summary>Drives the real ASP.NET Core pipeline (routing, model binding, JWT auth, the CQRS
/// dispatcher, EF Core) against a disposable SQL Server container to verify the AuthController
/// endpoints the spec calls for: register -> token, login -> token, and [Authorize] on /auth/me.</summary>
public sealed class AuthEndpointsTests : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Mssql"] = _container.GetConnectionString()
                });
            });
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IEventPublisher>();
                services.AddSingleton<IEventPublisher, NoOpEventPublisher>();
                services.RemoveAll<IAuditEventPublisher>();
                services.AddSingleton<IAuditEventPublisher, NoOpAuditEventPublisher>();
                services.RemoveAll<IEmailSender>();
                services.AddScoped<IEmailSender, NoOpEmailSender>();
                services.RemoveAll<ITenantRateLimiter>();
                services.AddSingleton<ITenantRateLimiter, AllowAllRateLimiter>();
            });
        });

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MssqlDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        _client = _factory.CreateClient();
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task Register_ThenMe_ReturnsTheAuthenticatedUser()
    {
        var registerResponse = await _client.PostAsJsonAsync("/auth/register", new
        {
            organizationName = "Acme Inc",
            email = "owner@acme.com",
            password = "Password1",
            firstName = "Ada",
            lastName = "Lovelace"
        });

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponsePayload>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth!.AccessToken));

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var meResponse = await _client.GetAsync("/auth/me");

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        var me = await meResponse.Content.ReadFromJsonAsync<UserPayload>();
        Assert.Equal("owner@acme.com", me!.Email);
        Assert.Equal("Owner", me.Role);
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ThenRefresh_IssuesANewTokenPair()
    {
        await _client.PostAsJsonAsync("/auth/register", new
        {
            organizationName = "Beta LLC",
            email = "user@beta.com",
            password = "Password1",
            firstName = "Grace",
            lastName = "Hopper"
        });

        var loginResponse = await _client.PostAsJsonAsync("/auth/login", new { email = "user@beta.com", password = "Password1" });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var login = await loginResponse.Content.ReadFromJsonAsync<AuthResponsePayload>();

        var refreshResponse = await _client.PostAsJsonAsync("/auth/refresh", new { refreshToken = login!.RefreshToken });
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<AuthResponsePayload>();

        Assert.NotEqual(login.RefreshToken, refreshed!.RefreshToken);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsBadRequest()
    {
        await _client.PostAsJsonAsync("/auth/register", new
        {
            organizationName = "Gamma Co",
            email = "user@gamma.com",
            password = "Password1",
            firstName = "Grace",
            lastName = "Hopper"
        });

        var response = await _client.PostAsJsonAsync("/auth/login", new { email = "user@gamma.com", password = "WrongPassword" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record AuthResponsePayload(string AccessToken, DateTimeOffset AccessTokenExpiresAt, string RefreshToken, UserPayload User);

    private sealed record UserPayload(Guid Id, Guid TenantId, string Email, string FirstName, string LastName, string Role);
}

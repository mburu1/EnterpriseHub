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

namespace EnterpriseHub.Tests.E2E.Workspace;

/// <summary>Drives Tenants (invitations) and Projects/Tasks endpoints through the real ASP.NET Core
/// pipeline against a disposable SQL Server container — the same infrastructure both bounded
/// contexts are persisted in, so one container covers the whole flow: register an owner, invite and
/// accept a second member, then have the owner create a project, add a milestone, create a task,
/// assign it to the new member, and move it through its status lifecycle.</summary>
public sealed class WorkspaceEndpointsTests : IAsyncLifetime
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

    private async Task<string> RegisterAndGetAccessTokenAsync(string org, string email)
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new
        {
            organizationName = org,
            email,
            password = "Password1",
            firstName = "Ada",
            lastName = "Lovelace"
        });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponsePayload>();
        return auth!.AccessToken;
    }

    [Fact]
    public async Task InviteMember_ThenAccept_CreatesAUserInTheSameTenant()
    {
        var ownerToken = await RegisterAndGetAccessTokenAsync("Acme Workspace Co", "owner@workspace.dev");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        var inviteResponse = await _client.PostAsJsonAsync("/tenants/invitations", new { email = "member@workspace.dev", role = "Member" });
        Assert.Equal(HttpStatusCode.OK, inviteResponse.StatusCode);
        var invitation = await inviteResponse.Content.ReadFromJsonAsync<InvitationPayload>();

        _client.DefaultRequestHeaders.Authorization = null;
        var acceptResponse = await _client.PostAsJsonAsync(
            $"/tenants/invitations/{invitation!.Id}/accept",
            new { password = "Password1", firstName = "Grace", lastName = "Hopper" });

        Assert.Equal(HttpStatusCode.OK, acceptResponse.StatusCode);
        var memberAuth = await acceptResponse.Content.ReadFromJsonAsync<AuthResponsePayload>();
        Assert.Equal(memberAuth!.User.TenantId, invitation.TenantId);
        Assert.Equal("Member", memberAuth.User.Role);
    }

    [Fact]
    public async Task InviteMember_WithoutOwnerOrAdminRole_ReturnsForbidden()
    {
        var ownerToken = await RegisterAndGetAccessTokenAsync("Beta Workspace Co", "owner@beta-workspace.dev");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var inviteResponse = await _client.PostAsJsonAsync("/tenants/invitations", new { email = "member@beta-workspace.dev", role = "Member" });
        var invitation = await inviteResponse.Content.ReadFromJsonAsync<InvitationPayload>();

        _client.DefaultRequestHeaders.Authorization = null;
        var acceptResponse = await _client.PostAsJsonAsync(
            $"/tenants/invitations/{invitation!.Id}/accept",
            new { password = "Password1", firstName = "Grace", lastName = "Hopper" });
        var memberAuth = await acceptResponse.Content.ReadFromJsonAsync<AuthResponsePayload>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberAuth!.AccessToken);
        var forbiddenResponse = await _client.PostAsJsonAsync("/tenants/invitations", new { email = "someone-else@beta-workspace.dev", role = "Member" });

        Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);
    }

    [Fact]
    public async Task ProjectAndTaskLifecycle_EndToEnd()
    {
        var ownerToken = await RegisterAndGetAccessTokenAsync("Gamma Workspace Co", "owner@gamma-workspace.dev");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        var inviteResponse = await _client.PostAsJsonAsync("/tenants/invitations", new { email = "assignee@gamma-workspace.dev", role = "Member" });
        var invitation = await inviteResponse.Content.ReadFromJsonAsync<InvitationPayload>();
        _client.DefaultRequestHeaders.Authorization = null;
        var acceptResponse = await _client.PostAsJsonAsync(
            $"/tenants/invitations/{invitation!.Id}/accept",
            new { password = "Password1", firstName = "Grace", lastName = "Hopper" });
        var assignee = await acceptResponse.Content.ReadFromJsonAsync<AuthResponsePayload>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        var createProjectResponse = await _client.PostAsJsonAsync("/projects", new { name = "Launch Plan", description = "Q1 launch" });
        Assert.Equal(HttpStatusCode.OK, createProjectResponse.StatusCode);
        var project = await createProjectResponse.Content.ReadFromJsonAsync<ProjectPayload>();

        var milestoneResponse = await _client.PostAsJsonAsync(
            $"/projects/{project!.Id}/milestones", new { name = "Beta release", dueDate = (DateOnly?)null });
        Assert.Equal(HttpStatusCode.OK, milestoneResponse.StatusCode);

        var statusResponse = await _client.PatchAsJsonAsync($"/projects/{project.Id}/status", new { status = "Active" });
        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);

        var createTaskResponse = await _client.PostAsJsonAsync(
            $"/projects/{project.Id}/tasks",
            new { title = "Write launch checklist", description = (string?)null, priority = "High", dueDate = (DateOnly?)null });
        Assert.Equal(HttpStatusCode.OK, createTaskResponse.StatusCode);
        var task = await createTaskResponse.Content.ReadFromJsonAsync<TaskPayload>();
        Assert.Equal("Todo", task!.Status);

        var assignResponse = await _client.PostAsJsonAsync($"/tasks/{task.Id}/assign", new { assigneeId = assignee!.User.Id });
        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
        var assignedTask = await assignResponse.Content.ReadFromJsonAsync<TaskPayload>();
        Assert.Equal(assignee.User.Id, assignedTask!.AssigneeId);

        var changeTaskStatusResponse = await _client.PatchAsJsonAsync($"/tasks/{task.Id}/status", new { status = "InProgress" });
        Assert.Equal(HttpStatusCode.OK, changeTaskStatusResponse.StatusCode);
        var inProgressTask = await changeTaskStatusResponse.Content.ReadFromJsonAsync<TaskPayload>();
        Assert.Equal("InProgress", inProgressTask!.Status);

        var listTasksResponse = await _client.GetAsync($"/projects/{project.Id}/tasks");
        Assert.Equal(HttpStatusCode.OK, listTasksResponse.StatusCode);
        var tasks = await listTasksResponse.Content.ReadFromJsonAsync<List<TaskPayload>>();
        Assert.Single(tasks!);
    }

    private sealed record AuthResponsePayload(string AccessToken, DateTimeOffset AccessTokenExpiresAt, string RefreshToken, UserPayload User);

    private sealed record UserPayload(Guid Id, Guid TenantId, string Email, string FirstName, string LastName, string Role);

    private sealed record InvitationPayload(Guid Id, Guid TenantId, string Email, string Role, bool Accepted, DateTimeOffset ExpiresAt);

    private sealed record ProjectPayload(Guid Id, Guid TenantId, string Name, string? Description, string Status);

    private sealed record TaskPayload(Guid Id, Guid TenantId, Guid ProjectId, string Title, string? Description, string Status, string Priority, Guid? AssigneeId, DateOnly? DueDate);
}

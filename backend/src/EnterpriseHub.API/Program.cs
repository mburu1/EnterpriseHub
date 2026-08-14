using System.Text;
using EnterpriseHub.API.Middleware;
using EnterpriseHub.API.Services;
using EnterpriseHub.Application;
using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Infrastructure;
using EnterpriseHub.Infrastructure.Options;
using EnterpriseHub.Infrastructure.Persistence.MySql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration).WriteTo.Console());

// ---- Application + Infrastructure ----
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---- MVC + OpenAPI ----
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HttpCurrentUserService>();

// ---- Problem details + global exception handling ----
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ---- JWT authentication ----
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "EnterpriseHub API";
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<TenantRateLimitingMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

// Best-effort: don't block startup on the billing DB being reachable yet (e.g. compose still
// bringing MySQL up); the app still serves everything else, and /billing/plans will just be
// empty until the store comes back and a later request/seed run succeeds.
using (var scope = app.Services.CreateScope())
{
    try
    {
        var mySqlDbContext = scope.ServiceProvider.GetRequiredService<MySqlDbContext>();
        await PlanSeeder.SeedAsync(mySqlDbContext);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Skipping plan seed — MySQL not reachable at startup.");
    }
}

app.Run();

// Exposed for WebApplicationFactory<Program> in the E2E test project.
public partial class Program;

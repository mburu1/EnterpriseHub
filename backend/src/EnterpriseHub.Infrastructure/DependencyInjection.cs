using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Domain.Billing;
using EnterpriseHub.Domain.Identity;
using EnterpriseHub.Domain.Notifications;
using EnterpriseHub.Domain.Projects;
using EnterpriseHub.Domain.Tenants;
using EnterpriseHub.Infrastructure.Cache;
using EnterpriseHub.Infrastructure.Emailing;
using EnterpriseHub.Infrastructure.Identity;
using EnterpriseHub.Infrastructure.Messaging.Kafka;
using EnterpriseHub.Infrastructure.Messaging.RabbitMQ;
using EnterpriseHub.Infrastructure.Mongo;
using EnterpriseHub.Infrastructure.Options;
using EnterpriseHub.Infrastructure.Persistence.Mssql;
using EnterpriseHub.Infrastructure.Persistence.Mssql.Repositories;
using EnterpriseHub.Infrastructure.Persistence.MySql;
using EnterpriseHub.Infrastructure.Persistence.Oracle;
using EnterpriseHub.Infrastructure.Persistence.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EnterpriseHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ---- Relational stores (polyglot persistence, see docs/adr/001-database-strategy.md) ----
        services.AddDbContext<MssqlDbContext>(o =>
            o.UseSqlServer(configuration.GetConnectionString("Mssql")));

        services.AddDbContext<PostgresDbContext>(o =>
            o.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.AddDbContext<MySqlDbContext>(o =>
            o.UseMySql(configuration.GetConnectionString("MySql"), ServerVersion.AutoDetect(configuration.GetConnectionString("MySql"))));

        services.AddDbContext<OracleDbContext>(o =>
            o.UseOracle(configuration.GetConnectionString("Oracle")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBillingUnitOfWork, MySqlUnitOfWork>();

        // ---- MongoDB: unstructured activity feed / attachments ----
        services.Configure<MongoOptions>(configuration.GetSection(MongoOptions.SectionName));
        services.AddSingleton(sp =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoOptions>>().Value;
            return new MongoContext(opts.ConnectionString, opts.Database);
        });
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // ---- Redis: cache, sessions, rate limiting ----
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "localhost:6379"));
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<ITenantRateLimiter, RedisSlidingWindowRateLimiter>();

        // ---- Messaging ----
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        services.AddSingleton<IAuditEventPublisher, KafkaAuditEventPublisher>();

        // ---- Email ----
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        // ---- Identity ----
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}

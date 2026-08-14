using System.Reflection;
using EnterpriseHub.Application.Common.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddScoped<ISender, Dispatcher>();
        services.AddValidatorsFromAssembly(assembly);

        foreach (var type in assembly.GetTypes().Where(t => t is { IsAbstract: false, IsInterface: false }))
        {
            foreach (var handlerInterface in type.GetInterfaces().Where(i =>
                         i.IsGenericType &&
                         (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                          i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))))
            {
                services.AddScoped(handlerInterface, type);
            }
        }

        return services;
    }
}

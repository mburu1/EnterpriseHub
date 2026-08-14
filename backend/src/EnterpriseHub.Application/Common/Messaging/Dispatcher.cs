using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseHub.Application.Common.Messaging;

/// <summary>
/// Minimal in-process mediator: resolves the ICommandHandler/IQueryHandler registered for the
/// runtime type of the request and invokes it. Avoids a third-party mediator dependency.
/// </summary>
public sealed class Dispatcher(IServiceProvider serviceProvider) : ISender
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> HandleMethodCache = new();

    public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct = default) =>
        Dispatch<TResponse>(typeof(ICommandHandler<,>), command, ct);

    public Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default) =>
        Dispatch<TResponse>(typeof(IQueryHandler<,>), query, ct);

    private Task<TResponse> Dispatch<TResponse>(Type openHandlerType, object request, CancellationToken ct)
    {
        var handlerType = openHandlerType.MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = serviceProvider.GetRequiredService(handlerType);

        var method = HandleMethodCache.GetOrAdd(handlerType, t => t.GetMethod("Handle")!);
        return (Task<TResponse>)method.Invoke(handler, [request, ct])!;
    }
}

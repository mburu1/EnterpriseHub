using System.Collections.Concurrent;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseHub.Application.Common.Messaging;

/// <summary>
/// Minimal in-process mediator: resolves the ICommandHandler/IQueryHandler registered for the
/// runtime type of the request, runs any registered FluentValidation validator first, then invokes
/// the handler. Avoids a third-party mediator dependency.
/// </summary>
public sealed class Dispatcher(IServiceProvider serviceProvider) : ISender
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> HandleMethodCache = new();

    public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct = default) =>
        Dispatch<TResponse>(typeof(ICommandHandler<,>), command, ct);

    public Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct = default) =>
        Dispatch<TResponse>(typeof(IQueryHandler<,>), query, ct);

    private async Task<TResponse> Dispatch<TResponse>(Type openHandlerType, object request, CancellationToken ct)
    {
        var requestType = request.GetType();

        var validatorType = typeof(IValidator<>).MakeGenericType(requestType);
        if (serviceProvider.GetService(validatorType) is IValidator validator)
        {
            var validationContext = new ValidationContext<object>(request);
            var validationResult = await validator.ValidateAsync(validationContext, ct);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
        }

        var handlerType = openHandlerType.MakeGenericType(requestType, typeof(TResponse));
        var handler = serviceProvider.GetRequiredService(handlerType);

        var method = HandleMethodCache.GetOrAdd(handlerType, t => t.GetMethod("Handle")!);
        return await (Task<TResponse>)method.Invoke(handler, [request, ct])!;
    }
}

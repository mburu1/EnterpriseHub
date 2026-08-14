using EnterpriseHub.Domain.Common;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseHub.API.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (statusCode, title, extensions) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                "One or more validation errors occurred.",
                new Dictionary<string, object?>
                {
                    ["errors"] = validationEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
                }),
            DomainException domainEx => (StatusCodes.Status400BadRequest, domainEx.Message, []),
            ForbiddenException forbiddenEx => (StatusCodes.Status403Forbidden, forbiddenEx.Message, []),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized.", []),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", [])
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception processing {Path}", httpContext.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Instance = httpContext.Request.Path
        };

        foreach (var (key, value) in extensions)
            problemDetails.Extensions[key] = value;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, ct);
        return true;
    }
}

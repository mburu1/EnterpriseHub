using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

// src/EnterpriseHub.Infrastructure/Common/DesignTimeConfiguration.cs



namespace EnterpriseHub.Infrastructure.Common;

/// <summary>
/// Resolves appsettings.json from the API host project at design time.
/// Used exclusively by IDesignTimeDbContextFactory implementations.
/// Never loaded at runtime — the API host owns config composition.
/// </summary>
internal static class DesignTimeConfiguration
{
    private const string ApiProjectName = "EnterpriseHub.Api";

    /// <summary>
    /// Walks up from the EF tooling working directory to locate the API host's
    /// appsettings.json. Does NOT load appsettings.Development.json so that
    /// real credentials stored in appsettings.json are always resolved.
    /// </summary>
    public static IConfiguration Build()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());

        // Traverse up until we find the API project folder or hit the root
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "src", ApiProjectName);
            if (Directory.Exists(candidate))
            {
                return new ConfigurationBuilder()
                    .SetBasePath(candidate)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .Build();
            }

            current = current.Parent;
        }

        throw new InvalidOperationException(
            $"Could not locate '{ApiProjectName}' under any 'src/' directory in the path tree. " +
            $"Ensure the API project exists at src/{ApiProjectName}/appsettings.json.");
    }

    public static string RequireConnectionString(this IConfiguration config, string name)
    {
        return config.GetConnectionString(name)
            ?? throw new InvalidOperationException(
                $"Connection string '{name}' is missing or empty in appsettings.json.");
    }
}

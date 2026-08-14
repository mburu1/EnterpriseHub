using System;
using System.Collections.Generic;
using System.Text;

// src/EnterpriseHub.Infrastructure/Persistence/ReportSnapshot.cs

namespace EnterpriseHub.Infrastructure.Persistence;

public sealed class ReportSnapshot
{
    public Guid Id { get; init; }
    public string ReportType { get; init; } = string.Empty;
    public string DataJson { get; init; } = string.Empty;
}

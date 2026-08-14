namespace EnterpriseHub.Application.Projects.Dtos;

public sealed record MilestoneDto(Guid Id, Guid ProjectId, string Name, DateOnly? DueDate, bool IsCompleted);

public sealed record ProjectDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    string Status,
    IReadOnlyCollection<MilestoneDto> Milestones);

public sealed record ProjectTaskDto(
    Guid Id,
    Guid TenantId,
    Guid ProjectId,
    string Title,
    string? Description,
    string Status,
    string Priority,
    Guid? AssigneeId,
    DateOnly? DueDate);

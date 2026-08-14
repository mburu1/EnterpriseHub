namespace EnterpriseHub.API.Contracts.Projects;

public sealed record CreateProjectRequest(string Name, string? Description);

public sealed record ChangeProjectStatusRequest(string Status);

public sealed record AddMilestoneRequest(string Name, DateOnly? DueDate);

public sealed record CreateTaskRequest(string Title, string? Description, string Priority, DateOnly? DueDate);

public sealed record AssignTaskRequest(Guid AssigneeId);

public sealed record ChangeTaskStatusRequest(string Status);

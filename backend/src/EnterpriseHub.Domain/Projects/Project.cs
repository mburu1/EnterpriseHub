using EnterpriseHub.Domain.Common;

namespace EnterpriseHub.Domain.Projects;

public sealed class Project : AggregateRoot<Guid>, ITenantScoped
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public ProjectStatus Status { get; private set; } = ProjectStatus.Planning;

    private readonly List<Milestone> _milestones = [];
    public IReadOnlyCollection<Milestone> Milestones => _milestones.AsReadOnly();

    private Project() { }

    public static Project Create(Guid tenantId, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Project name is required.");

        return new Project
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name.Trim(),
            Description = description
        };
    }

    public Milestone AddMilestone(string name, DateOnly? dueDate)
    {
        var milestone = Milestone.Create(Id, name, dueDate);
        _milestones.Add(milestone);
        return milestone;
    }

    public void ChangeStatus(ProjectStatus status)
    {
        Status = status;
        Touch();
    }
}

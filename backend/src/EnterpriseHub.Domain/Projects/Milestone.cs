using EnterpriseHub.Domain.Common;

namespace EnterpriseHub.Domain.Projects;

public sealed class Milestone : Entity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = null!;
    public DateOnly? DueDate { get; private set; }
    public bool IsCompleted { get; private set; }

    private Milestone() { }

    public static Milestone Create(Guid projectId, string name, DateOnly? dueDate) => new()
    {
        Id = Guid.NewGuid(),
        ProjectId = projectId,
        Name = name,
        DueDate = dueDate
    };

    public void Complete()
    {
        IsCompleted = true;
        Touch();
    }
}

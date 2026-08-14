using EnterpriseHub.Domain.Common;

namespace EnterpriseHub.Domain.Billing;

public sealed class Plan : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public BillingInterval Interval { get; private set; }

    private Plan() { }

    public static Plan Create(string name, Money price, BillingInterval interval) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Price = price,
        Interval = interval
    };
}

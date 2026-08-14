using EnterpriseHub.Domain.Billing;
using EnterpriseHub.Domain.Common;

namespace EnterpriseHub.Tests.Unit.Domain;

public class MoneyTests
{
    [Fact]
    public void Create_WithNegativeAmount_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Money.Create(-1));
    }

    [Fact]
    public void Create_UppercasesCurrency()
    {
        var money = Money.Create(9.99m, "usd");
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Equals_ComparesByAmountAndCurrency()
    {
        Assert.Equal(Money.Create(10, "USD"), Money.Create(10, "USD"));
        Assert.NotEqual(Money.Create(10, "USD"), Money.Create(10, "EUR"));
    }
}

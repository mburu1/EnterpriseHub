using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Identity;

namespace EnterpriseHub.Tests.Unit.Domain;

public class EmailTests
{
    [Theory]
    [InlineData("USER@Example.com", "user@example.com")]
    [InlineData("  user@example.com  ", "user@example.com")]
    public void Create_NormalizesToLowercaseAndTrims(string input, string expected)
    {
        var email = Email.Create(input);
        Assert.Equal(expected, email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    [InlineData("@nodomain.com")]
    public void Create_WithInvalidInput_ThrowsDomainException(string input)
    {
        Assert.Throws<DomainException>(() => Email.Create(input));
    }

    [Fact]
    public void Equals_IsCaseInsensitiveByValue()
    {
        var a = Email.Create("user@example.com");
        var b = Email.Create("USER@EXAMPLE.COM");

        Assert.Equal(a, b);
    }
}

using System.Text.RegularExpressions;
using EnterpriseHub.Domain.Common;

namespace EnterpriseHub.Domain.Identity;

public sealed partial class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty.");

        value = value.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(value))
            throw new DomainException($"'{value}' is not a valid email address.");

        return new Email(value);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}

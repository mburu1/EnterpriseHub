namespace EnterpriseHub.Domain.Common;

/// <summary>The actor is authenticated but not permitted to perform the requested action (403), as
/// distinct from <see cref="DomainException"/> (business rule violation, 400).</summary>
public class ForbiddenException(string message) : Exception(message);

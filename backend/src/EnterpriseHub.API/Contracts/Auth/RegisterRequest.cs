namespace EnterpriseHub.API.Contracts.Auth;

public sealed record RegisterRequest(
    string OrganizationName,
    string Email,
    string Password,
    string FirstName,
    string LastName);

namespace EnterpriseHub.API.Contracts.Tenants;

public sealed record AcceptInvitationRequest(string Password, string FirstName, string LastName);

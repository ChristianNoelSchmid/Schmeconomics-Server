namespace Schmeconomics.Api.Accounts;

public record class ToggleUserToAccountRequest(
    string AccountId,
    string UserId
);

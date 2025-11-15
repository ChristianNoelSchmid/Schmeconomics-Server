namespace Schmeconomics.Api.Users;

public record class CreateUserRequest(
    string Name,
    string Password
);
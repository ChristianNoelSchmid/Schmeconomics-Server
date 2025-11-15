
namespace Schmeconomics.Api.Users;

public record class UpdateUserRequest(
    string? UserId,
    string? Name,
    string? Password
);
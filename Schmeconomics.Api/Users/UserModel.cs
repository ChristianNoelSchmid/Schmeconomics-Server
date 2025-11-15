using Schmeconomics.Entities;

namespace Schmeconomics.Api.Users;

public record class UserModel(
    string Id,
    string Name,
    Role Role
) {
    public static explicit operator UserModel(User user) => new(
        user.Id,
        user.Name,
        user.Role
    );
}
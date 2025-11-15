namespace Schmeconomics.Api.Users;

/// <summary>
/// Retrieves the current, logged in user
/// </summary>
public interface ICurrentUser
{
    UserModel? User { get; }
}
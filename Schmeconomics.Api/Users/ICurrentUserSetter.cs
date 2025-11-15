namespace Schmeconomics.Api.Users;

/// <summary>
/// Allows setting of the current, logged in user
/// </summary>
public interface ICurrentUserSetter
{
    public UserModel? User { set; }
}
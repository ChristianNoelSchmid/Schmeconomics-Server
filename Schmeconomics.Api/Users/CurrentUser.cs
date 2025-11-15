namespace Schmeconomics.Api.Users;

/// <summary>
/// Defines a read/write storage for the current, logged in User
/// </summary>
public class CurrentUser : ICurrentUser, ICurrentUserSetter
{
    public UserModel? User { get; set; }
}
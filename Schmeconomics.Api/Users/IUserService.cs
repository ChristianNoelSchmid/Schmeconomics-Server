namespace Schmeconomics.Api.Users;

public interface IUserService
{
    public Task<UserModel?> GetUserFromIdAsync(string id, CancellationToken token = default);
    public Task<UserModel?> GetUserFromName(string name, CancellationToken token = default);
    public Task<UserModel> CreateUserAsync(string name, string password, CancellationToken token = default);
    public Task<UserModel?> DeleteUserAsync(string id, CancellationToken token = default);
    public Task<UserModel?> UpdateUserAsync(
        string id,
        string? name,
        string? password,
        CancellationToken token = default
    );
}
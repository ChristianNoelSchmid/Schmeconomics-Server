namespace Schmeconomics.Api.Users;

public interface IUserService
{
    Task<Result<IEnumerable<UserModel>>> GetAllUsersAsync();
    Task<Result> CreateAdminUser();
    Task<UserModel?> GetUserFromIdAsync(string id, CancellationToken token = default);
    Task<UserModel?> GetUserFromName(string name, CancellationToken token = default);
    Task<UserModel> CreateUserAsync(string name, string password, CancellationToken token = default);
    Task<UserModel?> DeleteUserAsync(string id, CancellationToken token = default);
    Task<UserModel?> UpdateUserAsync(
        string id,
        string? name,
        string? password,
        CancellationToken token = default
    );
}
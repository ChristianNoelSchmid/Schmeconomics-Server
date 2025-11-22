namespace Schmeconomics.Api.Auth;

public interface IAuthService
{
    Task<Result<AuthModel>> SignInAsync(string name, string password, string ipAddress, CancellationToken stopToken = default);
    Task<Result> SignOutAsync(string refreshToken, string ipAddress, CancellationToken stopToken = default);
    Task<Result<AuthModel>> RefreshTokenAsync(string ipAddress, string refreshToken, CancellationToken stopToken = default);
}

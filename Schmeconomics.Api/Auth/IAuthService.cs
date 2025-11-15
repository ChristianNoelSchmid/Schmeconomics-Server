namespace Schmeconomics.Api.Auth;

public interface IAuthService
{
    Task<AuthModel> SignInAsync(string name, string password, string ipAddress, CancellationToken stopToken = default);
    Task SignOutAsync(string refreshToken, string ipAddress, CancellationToken stopToken = default);
}

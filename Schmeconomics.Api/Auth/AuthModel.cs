
namespace Schmeconomics.Api.Auth;

public record class AuthModel(string AccessToken, string RefreshToken, DateTime ExpiresOnUtc);

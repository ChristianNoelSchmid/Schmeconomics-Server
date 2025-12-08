
namespace Schmeconomics.Api.Auth;

public record class AuthModel(string UserId, string AccessToken, string RefreshToken, DateTime ExpiresOnUtc);

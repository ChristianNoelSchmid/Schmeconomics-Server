namespace Schmeconomics.Api.Tokens;

public record class RefreshTokenResult(string Token, DateTime ExpiresOnUtc);

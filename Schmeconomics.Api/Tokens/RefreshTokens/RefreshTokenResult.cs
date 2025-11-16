namespace Schmeconomics.Api.Tokens.RefreshTokens;

public record class RefreshTokenResult(string Token, DateTime ExpiresOnUtc);

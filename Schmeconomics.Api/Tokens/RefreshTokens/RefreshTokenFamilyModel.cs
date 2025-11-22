using Schmeconomics.Api.Users;

namespace Schmeconomics.Api.Tokens.RefreshTokens;

public record class RefreshTokenFamilyModel(string Token, DateTime ExpiresOnUtc, UserModel User);

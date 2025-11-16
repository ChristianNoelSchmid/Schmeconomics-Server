using Schmeconomics.Api.Users;
using Schmeconomics.Entities;

namespace Schmeconomics.Api.Tokens.RefreshTokens;

public record class RefreshTokenFamilyModel(string Token, DateTime ExpiresOnUtc, UserModel User);

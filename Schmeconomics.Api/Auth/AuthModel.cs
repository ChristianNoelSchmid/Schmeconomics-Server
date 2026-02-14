
using Schmeconomics.Api.Users;

namespace Schmeconomics.Api.Auth;

public record class AuthModel(
    UserModel User, 
    string AccessToken, 
    string RefreshToken, 
    DateTime AccessTokenExpiresOnUtc,
    DateTime RefreshExpiresOnUtc
);

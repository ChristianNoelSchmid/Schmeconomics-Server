using Schmeconomics.Api.Users;

namespace Schmeconomics.Api.Auth;
public record SignInModel(
    UserModel UserModel,
    string AccessToken,
    DateTime ExpiresOnUtc
);
using Schmeconomics.Entities;
using Schmeconomics.Api.Users;
using Schmeconomics.Api.Categories;

namespace Schmeconomics.Api.Accounts;

public record class AccountModel(
    string Id,
    string Name,
    IEnumerable<CategoryModel> Categories,
    IEnumerable<UserModel> Users
)
{
    public static explicit operator AccountModel(Account from) => new(
        from.Id,
        from.Name,
        from.Categories.Select(c => (CategoryModel)c),
        from.AccountUsers.Where(au => au.User != null).Select(au => (UserModel)au.User!)
    );
}

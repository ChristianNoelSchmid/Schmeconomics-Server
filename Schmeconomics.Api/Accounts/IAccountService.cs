using Schmeconomics.Entities;

namespace Schmeconomics.Api.Accounts;

public interface IAccountService
{
    Task<Result<IEnumerable<AccountModel>>> GetAccountsForUserAsync(string userId, CancellationToken token = default);
    Task<Result<IEnumerable<AccountModel>>> GetAllAccountsAsync(CancellationToken token = default);
    Task<Result<AccountModel>> CreateAccountAsync(string name, CancellationToken token = default);
    Task<Result> ToggleUserToAccountAsync(string accountId, string userId, CancellationToken token = default);
    Task<Result> DeleteAccountAsync(string id, CancellationToken token = default);
    Task<Result<AccountModel>> UpdateAccountAsync(string id, string? name, CancellationToken token = default);
}

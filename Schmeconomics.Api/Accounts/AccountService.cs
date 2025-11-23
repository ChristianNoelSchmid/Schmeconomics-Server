using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Schmeconomics.Entities;

namespace Schmeconomics.Api.Accounts;

public class AccountService(
    SchmeconomicsDbContext db
) : IAccountService {
    private readonly SchmeconomicsDbContext _db = db;

    public async Task<Result<IEnumerable<AccountModel>>> GetAccountsForUserAsync(string userId, CancellationToken token = default)
    {
        try 
        {
            var user = await _db.Users.FindAsync([userId], token);
            if(user is null) return new AccountServiceError.UserNotFound(userId);

            return await _db.Accounts
                .Include(a => a.Categories)
                .Include(a => a.AccountUsers)
                .ThenInclude(au => au.User)
                .Where(a => a.AccountUsers.Any(au => au.UserId == userId))
                .Select(am => (AccountModel)am)
                .ToListAsync(token);
        } 
        catch(DbException ex)
        {
            throw new AccountServiceException.DbException(ex);
        }
    }


    public async Task<Result<AccountModel>> CreateAccountAsync(string name, CancellationToken token = default)
    {
        try
        {
            // Check if account with this name already exists
            if (await _db.Accounts.Where(a => a.Name == name).FirstOrDefaultAsync(token) != null)
                return new AccountServiceError.AccountAlreadyExists(name);
            
            var account = new Account { Id = Guid.NewGuid().ToString(), Name = name };
            
            _db.Accounts.Add(account);
            
            // Add all admin users to the account
            var adminUsers = await _db.Users
                .Where(u => u.Role == Role.Admin)
                .Select(u => new AccountUser { AccountId = account.Id, UserId = u.Id })
                .ToListAsync(token);
            
            _db.AccountUsers.AddRange(adminUsers);
            
            await _db.SaveChangesAsync(token);

            return (AccountModel)account;
        }
        catch(DbException ex)
        {
            throw new AccountServiceException.DbException(ex);
        }
    }

    public async Task<Result> ToggleUserToAccountAsync(string accountId, string userId, CancellationToken token = default)
    {
        try
        {
            // Check if account exists
            var account = await _db.Accounts.FindAsync([accountId], token);
            if (account == null) return new AccountServiceError.AccountNotFound(accountId);
            
            // Check if user exists
            var user = await _db.Users.FindAsync([userId], token);
            if (user == null) return new AccountServiceError.UserNotFound(userId);
            
            // Check if association already exists
            // If it does, remove it
            var accountUser = await _db.AccountUsers
                .Where(au => au.AccountId == accountId && au.UserId == userId)
                .FirstOrDefaultAsync(token);

            if(accountUser != null)
            {
                // Check if the user being removed is an Admin
                if (user.Role == Role.Admin)
                {
                    return new AccountServiceError.AdminUserCannotBeRemovedFromAccount();
                }
                
                _db.AccountUsers.Remove(accountUser);
            }
            else 
            {
                var newAccountUser = new AccountUser { AccountId = accountId, UserId = userId };
                _db.AccountUsers.Add(newAccountUser);
            }
            await _db.SaveChangesAsync(token);
            
            return Result.Ok();
        }
        catch(DbException ex)
        {
            throw new AccountServiceException.DbException(ex);
        }
    }

    public async Task<Result> DeleteAccountAsync(string id, CancellationToken token = default)
    {
        try
        {
            var account = await _db.Accounts.FindAsync([id], token);
            if (account == null) return new AccountServiceError.AccountNotFound(id);
            
            _db.Accounts.Remove(account);
            await _db.SaveChangesAsync(token);
            
            return Result.Ok();
        }
        catch(DbException ex)
        {
            throw new AccountServiceException.DbException(ex);
        }
    }

    public async Task<Result<AccountModel>> UpdateAccountAsync(string id, string? name, CancellationToken token = default)
    {
        try
        {
            var account = await _db.Accounts.FindAsync([id], token);
            if (account == null) return new AccountServiceError.AccountNotFound(id);
            
            if (name != null)
            {
                // Check if another account with this name already exists
                if (await _db.Accounts
                    .Where(a => a.Name == name && a.Id != id)
                    .FirstOrDefaultAsync(token) != null)
                {
                    return new AccountServiceError.AccountAlreadyExists(name);
                }
                
                account.Name = name;
            }
            
            _db.Accounts.Update(account);
            await _db.SaveChangesAsync(token);
            
            return (AccountModel)account;
        }
        catch(DbException ex)
        {
            throw new AccountServiceException.DbException(ex);
        }
    }
}

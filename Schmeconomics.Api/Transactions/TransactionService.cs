using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Schmeconomics.Entities;

namespace Schmeconomics.Api.Transactions;

public class TransactionService(
    SchmeconomicsDbContext db
) : ITransactionService {
    private readonly SchmeconomicsDbContext _db = db;

    public async Task<Result<IReadOnlyList<TransactionModel>>> GetTransactionsByAccountAsync(string accountId, int page, int pageSize, CancellationToken token = default)
    {
        try
        {
            // Check if account exists
            var account = await _db.Accounts.FindAsync([accountId], token);
            if(account is null) return new TransactionServiceError.AccountNotFound(accountId);

            var transactions = await _db.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.TimestampUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(token);
                
            return transactions.Select(t => (TransactionModel)t).ToList();
        }
        catch(DbException ex)
        {
            throw new TransactionServiceException.DbException(ex);
        }
    }

    public async Task<Result<IReadOnlyList<TransactionModel>>> GetTransactionsByCategoryAsync(string accountId, string categoryId, int page, int pageSize, CancellationToken token = default)
    {
        try
        {
            // Check if account exists
            var account = await _db.Accounts.FindAsync([accountId], token);
            if(account is null) return new TransactionServiceError.AccountNotFound(accountId);

            // Check if category exists
            var category = await _db.Categories.FindAsync([categoryId], token);
            if(category is null || category.AccountId != accountId) return new TransactionServiceError.CategoryNotFound(categoryId);

            var transactions = await _db.Transactions
                .Where(t => t.CategoryId == categoryId)
                .OrderByDescending(t => t.TimestampUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(token);
                
            return transactions.Select(t => (TransactionModel)t).ToList();
        }
        catch(DbException ex)
        {
            throw new TransactionServiceException.DbException(ex);
        }
    }

    public async Task<Result> CreateTransactionsAsync(string accountId, IReadOnlyList<CreateTransactionRequest> requests, CancellationToken token = default)
    {
        try
        {
            // Check if account exists
            var account = await _db.Accounts.FindAsync([accountId], token);
            if(account is null) return new TransactionServiceError.AccountNotFound(accountId);

            foreach (var request in requests)
            {
                // Check if category exists
                var category = await _db.Categories
                    .Where(c => c.AccountId == accountId)
                    .FirstOrDefaultAsync(c => c.Id == request.CategoryId, token);

                if(category is null) return new TransactionServiceError.CategoryNotFound(request.CategoryId);

                // Create transaction
                var transaction = new Transaction 
                { 
                    Id = Guid.NewGuid().ToString(), 
                    CategoryId = request.CategoryId,
                    AccountId = accountId,
                    TimestampUtc = request.TimestampUtc,
                    Amount = request.Amount,
                    Notes = request.Notes
                };
                
                _db.Transactions.Add(transaction);
                
                // Update category balance
                category.Balance += transaction.Amount;
            }
            
            await _db.SaveChangesAsync(token);
            return Result.Ok();
        }
        catch(DbException ex)
        {
            throw new TransactionServiceException.DbException(ex);
        }
    }

    public async Task<Result> DeleteTransactionsAsync(string accountId, IReadOnlyList<string> transactionIds, CancellationToken token = default)
    {
        try
        {
            // Check if account exists
            var account = await _db.Accounts.FindAsync([accountId], token);
            if(account is null) return new TransactionServiceError.AccountNotFound(accountId);

            foreach (var transactionId in transactionIds)
            {
                // Check if transaction exists and belongs to the account
                var transaction = await _db.Transactions
                    .FirstOrDefaultAsync(t => t.Id == transactionId && t.AccountId == accountId, token);

                if(transaction is null) return new TransactionServiceError.TransactionNotFound(transactionId);

                // Remove transaction
                _db.Transactions.Remove(transaction);
                
                // Update category balance (reverse the amount)
                transaction.Category!.Balance -= transaction.Amount;
            }
            
            await _db.SaveChangesAsync(token);
            return Result.Ok();
        }
        catch(DbException ex)
        {
            throw new TransactionServiceException.DbException(ex);
        }
    }

    public async Task<Result<TransactionModel>> UpdateTransactionAsync(string accountId, string transactionId, UpdateTransactionRequest request, CancellationToken token = default)
    {
        try
        {
            // Check if account exists
            var account = await _db.Accounts.FindAsync([accountId], token);
            if(account is null) return new TransactionServiceError.AccountNotFound(accountId);

            var transaction = await _db.Transactions.FindAsync([transactionId], token);
            if(transaction is null) return new TransactionServiceError.TransactionNotFound(transactionId);
            
            // Check if transaction belongs to the account
            if(transaction.AccountId != accountId) 
                return new TransactionServiceError.TransactionNotFound(transactionId);

            // Store original values for balance adjustment
            var originalCategoryId = transaction.CategoryId;
            var originalAmount = transaction.Amount;

            // Update transaction fields if provided
            if (request.CategoryId != null) transaction.CategoryId = request.CategoryId;
            if (request.TimestampUtc != null) transaction.TimestampUtc = request.TimestampUtc.Value;
            if (request.Amount != null) transaction.Amount = request.Amount.Value;
            if (request.Notes != null) transaction.Notes = request.Notes;

            // If category or amount changed, adjust balances accordingly
            if (request.CategoryId != null || request.Amount != null)
            {
                // First, reverse the original category's balance
                var originalCategory = await _db.Categories.FindAsync([originalCategoryId], token);
                if(originalCategory is not null)
                {
                    originalCategory.Balance -= originalAmount;
                }

                // Then apply new category's balance adjustment
                if (request.CategoryId != null && request.CategoryId != originalCategoryId)
                {
                    var newCategory = await _db.Categories.FindAsync([request.CategoryId], token);
                    if(newCategory is not null)
                    {
                        newCategory.Balance += transaction.Amount;
                    }
                }
                else if (request.Amount != null)
                {
                    // Same category, but amount changed
                    var currentCategory = await _db.Categories.FindAsync([transaction.CategoryId], token);
                    if(currentCategory is not null)
                    {
                        currentCategory.Balance += (transaction.Amount - originalAmount);
                    }
                }
            }

            _db.Transactions.Update(transaction);
            await _db.SaveChangesAsync(token);
            
            return (TransactionModel)transaction;
        }
        catch(DbException ex)
        {
            throw new TransactionServiceException.DbException(ex);
        }
    }
}

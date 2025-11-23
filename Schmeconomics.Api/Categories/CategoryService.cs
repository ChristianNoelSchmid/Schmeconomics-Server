using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Schmeconomics.Entities;

namespace Schmeconomics.Api.Categories;

public class CategoryService(
    SchmeconomicsDbContext db
) : ICategoryService {
    private readonly SchmeconomicsDbContext _db = db;

    public async Task<Result<CategoryModel>> CreateCategoryAsync(string accountId, string name, int balance, int refillValue, CancellationToken token = default)
    {
        try
        {
            // Check if account exists
            var account = await _db.Accounts.FindAsync([accountId], token);
            if(account is null) return new CategoryServiceError.AccountNotFound(accountId);

            // Check if category with this name already exists for the account
            if (await _db.Categories
                .Where(c => c.AccountId == accountId && c.Name == name)
                .FirstOrDefaultAsync(token) != null)
                return new CategoryServiceError.CategoryAlreadyExists(name);

            var category = new Category 
            { 
                Id = Guid.NewGuid().ToString(), 
                Name = name, 
                Balance = balance,
                RefillValue = refillValue,
                AccountId = accountId
            };
            
            _db.Categories.Add(category);
            await _db.SaveChangesAsync(token);

            return (CategoryModel)category;
        }
        catch(DbException ex)
        {
            throw new CategoryServiceException.DbException(ex);
        }
    }

    public async Task<Result> DeleteCategoryAsync(string id, CancellationToken token = default)
    {
        try
        {
            var category = await _db.Categories.FindAsync([id], token);
            if (category == null) return new CategoryServiceError.CategoryNotFound(id);
            
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync(token);
            
            return Result.Ok();
        }
        catch(DbException ex)
        {
            throw new CategoryServiceException.DbException(ex);
        }
    }

    public async Task<Result<CategoryModel>> UpdateCategoryAsync(string id, string? name, int? balance, int? refillValue, CancellationToken token = default)
    {
        try
        {
            var category = await _db.Categories.FindAsync([id], token);
            if (category == null) return new CategoryServiceError.CategoryNotFound(id);
            
            // Check if a category with this name already exists for the account (excluding current category)
            if (name != null && name != category.Name)
            {
                var existingCategory = await _db.Categories
                    .Where(c => c.AccountId == category.AccountId && c.Name == name)
                    .FirstOrDefaultAsync(token);
                    
                if (existingCategory != null)
                {
                    return new CategoryServiceError.CategoryAlreadyExists(name);
                }
            }
            
            // Update fields if provided
            if (name != null) category.Name = name;
            if (balance != null) category.Balance = balance.Value;
            if (refillValue != null) category.RefillValue = refillValue.Value;
            
            _db.Categories.Update(category);
            await _db.SaveChangesAsync(token);
            
            return (CategoryModel)category;
        }
        catch(DbException ex)
        {
            throw new CategoryServiceException.DbException(ex);
        }
    }

    public async Task<Result> UpdateCategoryOrdersAsync(string accountId, IReadOnlyList<string> categoryIds, CancellationToken token = default)
    {
        try
        {
            // Check if account exists
            var account = await _db.Accounts
                .Include(a => a.Categories)
                .Where(a => a.Id == accountId)
                .FirstOrDefaultAsync(token);

            if(account is null) 
            {
                return new CategoryServiceError.AccountNotFound(accountId);
            }

            // Check if all existing categories are included in the provided list
            if (account.Categories.Select(c => c.Id).Except(categoryIds).Any())
            {
                return new CategoryServiceError.MissingCategories();
            }

            // Update the order for each category
            for(int i = 0; i < categoryIds.Count; i += 1)
            {
                var category = account.Categories.FirstOrDefault(c => c.Id == categoryIds[i]);
                if (category != null) 
                {
                    category.Order = i;
                }
            }

            await _db.SaveChangesAsync(token);
            return Result.Ok();
        }
        catch(DbException ex)
        {
            throw new CategoryServiceException.DbException(ex);
        }
    }
}

using Schmeconomics.Entities;

namespace Schmeconomics.Api.Categories;

public interface ICategoryService
{
    Task<Result<IEnumerable<CategoryModel>>> GetCategoriesForAccountAsync(string accountId, CancellationToken token = default);
    Task<Result<CategoryModel>> CreateCategoryAsync(string accountId, string name, int balance, int refillValue, CancellationToken token = default);
    Task<Result> DeleteCategoryAsync(string id, CancellationToken token = default);
    Task<Result<CategoryModel>> UpdateCategoryAsync(string id, string? name, int? balance, int? refillValue, CancellationToken token = default);
    Task<Result> UpdateCategoriesOrderAsync(string accountId, IEnumerable<CategoryModel> categories, CancellationToken token = default);
}

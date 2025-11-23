namespace Schmeconomics.Api.Categories;

public interface ICategoryService
{
    Task<Result<CategoryModel>> CreateCategoryAsync(string accountId, string name, int balance, int refillValue, CancellationToken token = default);
    Task<Result> DeleteCategoryAsync(string id, CancellationToken token = default);
    Task<Result<CategoryModel>> UpdateCategoryAsync(string id, string? name, int? balance, int? refillValue, CancellationToken token = default);
    Task<Result> UpdateCategoryOrdersAsync(string accountId, IReadOnlyList<string> categoryIds, CancellationToken token = default);
}

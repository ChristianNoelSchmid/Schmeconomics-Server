namespace Schmeconomics.Api.Categories;

public record class UpdateCategoryRequest(
    string? Name = null,
    int? Balance = null,
    int? RefillValue = null
);

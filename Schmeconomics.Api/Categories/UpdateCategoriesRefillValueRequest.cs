namespace Schmeconomics.Api.Categories;

public record class UpdateCategoriesRefillValueRequest(
    string AccountId,
    IReadOnlyList<CategoryRefillValueUpdate> RefillValues
);

public record class CategoryRefillValueUpdate(
    string CategoryId,
    int RefillValue
);

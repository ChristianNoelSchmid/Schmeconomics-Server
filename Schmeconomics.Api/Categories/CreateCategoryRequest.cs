namespace Schmeconomics.Api.Categories;

public record class CreateCategoryRequest(
    string AccountId,
    string Name,
    int Balance,
    int RefillValue
);

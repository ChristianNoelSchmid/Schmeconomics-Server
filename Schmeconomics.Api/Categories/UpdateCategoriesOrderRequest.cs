namespace Schmeconomics.Api.Categories;

public record class UpdateCategoriesOrderRequest(
    string AccountId,
    IReadOnlyList<string> CategoryIds
);

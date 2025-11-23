using Schmeconomics.Entities;

namespace Schmeconomics.Api.Categories;

public record class CategoryModel(
    string Id,
    string Name,
    int Balance,
    int RefillValue,
    int Order
) {
    public static explicit operator CategoryModel(Category from) => new(
        from.Id,
        from.Name,
        from.Balance,
        from.RefillValue,
        from.Order
    );
}

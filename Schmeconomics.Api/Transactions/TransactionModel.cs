using Schmeconomics.Entities;

namespace Schmeconomics.Api.Transactions;

public record class TransactionModel(
    string Id,
    string CategoryId,
    DateTime TimestampUtc,
    int Amount,
    string? Notes
) {
    public static explicit operator TransactionModel(Transaction from) => new(
        from.Id,
        from.CategoryId,
        from.TimestampUtc,
        from.Amount,
        from.Notes
    );
}

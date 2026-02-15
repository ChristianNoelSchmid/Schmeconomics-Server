using Schmeconomics.Entities;

namespace Schmeconomics.Api.Transactions;

public record class TransactionModel(
    string Id,
    string Creator,
    string CategoryName,
    DateTime TimestampUtc,
    int Amount,
    string? Notes
) {
    public static explicit operator TransactionModel(Transaction from) => new(
        from.Id,
        from.Creator?.Name ?? throw new ArgumentException(
            $"Conversion from {nameof(Transaction)} to {nameof(TransactionModel)} " + 
            $"requires joining with {nameof(User)} foreign key."
        ),
        from.Category?.Name ?? throw new ArgumentException(
            $"Conversion from {nameof(Transaction)} to {nameof(TransactionModel)} " + 
            $"requires joining with {nameof(Category)} foreign key."
        ),
        from.TimestampUtc,
        from.Amount,
        from.Notes
    );
}

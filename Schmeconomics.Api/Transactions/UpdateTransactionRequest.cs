namespace Schmeconomics.Api.Transactions;

public record class UpdateTransactionRequest(
    string? CategoryId,
    DateTime? TimestampUtc,
    int? Amount,
    string? Notes
);

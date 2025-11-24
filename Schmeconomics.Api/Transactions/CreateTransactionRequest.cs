namespace Schmeconomics.Api.Transactions;

public record class CreateTransactionRequest(
    string CategoryId,
    DateTime TimestampUtc,
    int Amount,
    string? Notes
);

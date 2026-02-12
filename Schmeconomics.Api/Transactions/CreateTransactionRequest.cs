namespace Schmeconomics.Api.Transactions;

public record class CreateTransactionRequest(
    string CategoryId,
    int Amount,
    string? Notes
);

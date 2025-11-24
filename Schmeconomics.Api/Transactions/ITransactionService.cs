namespace Schmeconomics.Api.Transactions;

public interface ITransactionService
{
    Task<Result<IReadOnlyList<TransactionModel>>> GetTransactionsByAccountAsync(string accountId, int page, int pageSize, CancellationToken token = default);
    Task<Result<IReadOnlyList<TransactionModel>>> GetTransactionsByCategoryAsync(string categoryId, int page, int pageSize, CancellationToken token = default);
    Task<Result> CreateTransactionsAsync(string accountId, IReadOnlyList<CreateTransactionRequest> requests, CancellationToken token = default);
    Task<Result> DeleteTransactionsAsync(string accountId, IReadOnlyList<string> transactionIds, CancellationToken token = default);
    Task<Result<TransactionModel>> UpdateTransactionAsync(string accountId, string transactionId, UpdateTransactionRequest request, CancellationToken token = default);
}

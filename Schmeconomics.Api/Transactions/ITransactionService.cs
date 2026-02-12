namespace Schmeconomics.Api.Transactions;

public interface ITransactionService
{
    Task<Result<IReadOnlyList<TransactionModel>>> GetTransactionsByAccountAsync(string userId, string accountId, int page, int pageSize, CancellationToken token = default);
    Task<Result<IReadOnlyList<TransactionModel>>> GetTransactionsByCategoryAsync(string userId, string accountId, string categoryId, int page, int pageSize, CancellationToken token = default);
    Task<Result> CreateTransactionsAsync(string userId, string accountId, IReadOnlyList<CreateTransactionRequest> requests, CancellationToken token = default);
    Task<Result> DeleteTransactionsAsync(string userId, string accountId, IReadOnlyList<string> transactionIds, CancellationToken token = default);
    Task<Result<TransactionModel>> UpdateTransactionAsync(string userId, string accountId, string transactionId, UpdateTransactionRequest request, CancellationToken token = default);
}

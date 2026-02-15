using Microsoft.AspNetCore.Mvc;
using Schmeconomics.Api.Auth;
using Schmeconomics.Api.Transactions;
using Schmeconomics.Api.Users;

namespace Schmeconomics.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Role.User)]
public class TransactionController(
    ITransactionService _transactionService,
    ICurrentUser _currentUser
) : ControllerBase {
    [HttpGet("{accountId}")]
    [ProducesResponseType<TransactionModel[]>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountTransactions(
        string accountId,
        [FromQuery] string? categoryId,
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 10
    ) {
        if(accountId == null)
            return BadRequest("Please provide an `accountId`");

        Result<IReadOnlyList<TransactionModel>> result;

        if(categoryId == null) 
            result = await _transactionService.GetTransactionsByAccountAsync(
                _currentUser.User!.Id, accountId, page, pageSize
            );
        else 
            result = await _transactionService.GetTransactionsByCategoryAsync(
                _currentUser.User!.Id, accountId, categoryId, page, pageSize
            );

        if(result.IsError)
            return NotFound(result.Error.Message);
            
        return Ok(result.Value);
    }

    [HttpPost("{accountId}")]
    public async Task<IActionResult> CreateTransactionsAsync(
        string accountId,
        [FromBody] IReadOnlyList<CreateTransactionRequest> requests,
        CancellationToken stopToken = default
    ) {
        var result = await _transactionService.CreateTransactionsAsync(
            _currentUser.User!.Id,
            accountId,
            requests, 
            stopToken
        );
        
        if (result.IsOk) return Ok();
        else return NotFound(result.Error.Message);
    }

    [HttpDelete("{transactionId}")]
    public async Task<IActionResult> DeleteTransactionAsync(
        string transactionId,
        CancellationToken stopToken = default
    ) {
        var result = await _transactionService.DeleteTransactionsAsync(
            _currentUser.User!.Id,
            transactionId, 
            [transactionId], 
            stopToken
        );
        
        if (result.IsOk) return Ok();
        else return NotFound(result.Error.Message);
    }

    [HttpPut("{transactionId}")]
    [ProducesResponseType<TransactionModel>(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTransactionAsync(
        string transactionId,
        UpdateTransactionRequest request,
        CancellationToken stopToken = default
    ) {
        var result = await _transactionService.UpdateTransactionAsync(
            _currentUser.User!.Id, 
            transactionId,
            transactionId, 
            request, 
            stopToken
        );
        
        if (!result.IsOk) 
            return NotFound(result.Error.Message);

        return Ok(result.Value);
    }
}
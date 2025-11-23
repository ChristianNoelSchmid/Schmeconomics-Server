using Microsoft.AspNetCore.Mvc;
using Schmeconomics.Api.Accounts;
using Schmeconomics.Api.Auth;
using Schmeconomics.Api.Users;

namespace Schmeconomics.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Role.Admin)]
public class AccountController(
    IAccountService _accountService,
    ICurrentUser _currentUser
) : ControllerBase
{
    [HttpGet("Get")]
    [Authorize(Role.User)]
    public async Task<IActionResult> GetAccountsForUserAsync(
        CancellationToken stopToken = default
    ) {
        if (_currentUser.User == null)
            return Unauthorized();
            
        var accounts = await _accountService.GetAccountsForUserAsync(_currentUser.User.Id, stopToken);
        if(accounts.IsOk) return Ok(accounts.Value);
        else return NotFound(accounts.Error);
    }

    [HttpGet("All")]
    public async Task<IActionResult> GetAllAccountsAsync(
        CancellationToken stopToken = default)
    {
        var accounts = await _accountService.GetAllAccountsAsync(stopToken);
        
        return Ok(accounts.Value);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateAccountAsync(
        string name,
        CancellationToken stopToken = default)
    {
        var account = await _accountService.CreateAccountAsync(name, stopToken);
        
        if (account.IsOk) return Ok(account.Value);
        else return BadRequest(account.Error.Message);
    }

    [HttpPost("ToggleUser")]
    public async Task<IActionResult> ToggleUserToAccountAsync(
        ToggleUserToAccountRequest request,
        CancellationToken stopToken = default)
    {
        var result = await _accountService.ToggleUserToAccountAsync(request.AccountId, request.UserId, stopToken);
        
        if (result.IsOk) return Ok();
        else return result.Error switch
        {
            AccountServiceError.AccountNotFound => NotFound(result.Error.Message),
            AccountServiceError.UserNotFound or _ => NotFound(result.Error.Message),
        };
    }

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> DeleteAccountAsync(
        string id,
        CancellationToken stopToken = default)
    {
        var result = await _accountService.DeleteAccountAsync(id, stopToken);
        
        if (result.IsOk) return Ok();
        else return NotFound(result.Error.Message);
        
    }

    [HttpPut("Update/{id}")]
    public async Task<IActionResult> UpdateAccountAsync(
        string id,
        UpdateAccountRequest request,
        CancellationToken stopToken = default)
    {
        var account = await _accountService.UpdateAccountAsync(id, request.Name, stopToken);
        
        if (account.IsOk) return Ok(account.Value);
        else return account.Error switch
        {
            AccountServiceError.AccountNotFound => NotFound(account.Error.Message),
            AccountServiceError.AccountAlreadyExists or _ => BadRequest(account.Error.Message),
        };
    }
}

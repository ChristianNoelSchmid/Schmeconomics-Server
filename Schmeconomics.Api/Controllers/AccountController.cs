using Microsoft.AspNetCore.Mvc;
using Schmeconomics.Api.Accounts;
using Schmeconomics.Api.Auth;
using Schmeconomics.Api.Users;

namespace Schmeconomics.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController(
    IAccountService _accountService,
    ICurrentUser _currentUser
) : ControllerBase
{
    [HttpGet("All")]
    [Authorize(Role.User)]
    [ProducesResponseType<IEnumerable<AccountModel>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountsForUserAsync(
        CancellationToken stopToken = default
    ) {
        if (_currentUser.User == null)
            return Unauthorized();
            
        var accounts = await _accountService.GetAccountsForUserAsync(_currentUser.User.Id, stopToken);
        if(accounts.IsOk) return Ok(accounts.Value);
        else return NotFound(accounts.Error);
    }


    [HttpPost("Create")]
    [Authorize(Role.Admin)]
    [ProducesResponseType<AccountModel>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateAccountAsync(
        string name,
        CancellationToken stopToken = default)
    {
        var account = await _accountService.CreateAccountAsync(name, stopToken);
        
        if (account.IsOk) return Ok(account.Value);
        else return BadRequest(account.Error.Message);
    }

    [HttpPost("ToggleUser")]
    [Authorize(Role.Admin)]
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
    [Authorize(Role.Admin)]
    public async Task<IActionResult> DeleteAccountAsync(
        string id,
        CancellationToken stopToken = default)
    {
        var result = await _accountService.DeleteAccountAsync(id, stopToken);
        
        if (result.IsOk) return Ok();
        else return NotFound(result.Error.Message);
        
    }

    [HttpPut("Update/{id}")]
    [ProducesResponseType<AccountModel>(StatusCodes.Status200OK)]
    [Authorize(Role.Admin)]
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

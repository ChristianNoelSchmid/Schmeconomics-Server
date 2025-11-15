using Microsoft.AspNetCore.Mvc;
using Schmeconomics.Api.Auth;
using Schmeconomics.Api.Users;

namespace Schmeconomics.Api.Controllers;

[ApiController, Authorize(Role.User), Route("[controller]")]
public class UserController (
    IUserService _userService,
    ICurrentUser _current
) : ControllerBase {

    [HttpGet("/")]
    public IActionResult GetUser()
    {
        return Ok(_current.User);
    }

    [HttpGet("/ById/{id}")]
    public async Task<IActionResult> GetUserById(
        string id,
        CancellationToken stopToken
    ) {
        var user = await _userService.GetUserFromIdAsync(id, stopToken);
        if(user == null) return NotFound();
        else return Ok(user);
    }

    [HttpGet("/ByName/{name}")]
    public async Task<IActionResult> GetUserByName(
        string name,
        CancellationToken stopToken
    ) {
        var user = await _userService.GetUserFromName(name, stopToken);
        if(user == null) return NotFound();
        else return Ok(user);
    }

    [HttpPost("/Create"), Authorize(Role.Admin)]
    public async Task<IActionResult> CreateUser(
        CreateUserRequest request,
        CancellationToken stopToken
    ) {
        var user = await _userService.CreateUserAsync(request.Name, request.Password, stopToken);
        return Ok(user);
    }

    [HttpDelete("/Delete/{userId}"), Authorize(Role.Admin)]
    public async Task<IActionResult> DeleteUser(
        string userId,
        CancellationToken stopToken
    ) {
        var user = await _userService.DeleteUserAsync(userId, stopToken);
        if(user == null) return NotFound();
        else return Ok(user);
    }

    [HttpPut("/Update")]
    public async Task<IActionResult> UpdateUser(
        UpdateUserRequest request,
        CancellationToken stopToken
    ) {
        if(request.UserId != null && _current.User!.Role != Role.Admin) 
            return Unauthorized();

        var userId = request.UserId ?? _current.User!.Id;
        var user = await _userService.UpdateUserAsync(userId, request.Name, request.Password, stopToken);

        return Ok(user);
    }
}

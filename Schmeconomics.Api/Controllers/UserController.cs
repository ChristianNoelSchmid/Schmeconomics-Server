using Microsoft.AspNetCore.Mvc;
using Schmeconomics.Api.Auth;
using Schmeconomics.Api.Users;

namespace Schmeconomics.Api.Controllers;

[ApiController]
[Authorize(Role.User)]
[Route("[controller]")]
public class UserController (
    IUserService _userService,
    ICurrentUser _current
) : ControllerBase {

    [HttpGet("All")]
    [ProducesResponseType<IEnumerable<UserModel>>(StatusCodes.Status200OK)]
    [Authorize(Role.Admin)]
    public async Task<IActionResult> GetAllUsersAsync()
    {
        var usersResult = await _userService.GetAllUsersAsync();
        return Ok(usersResult.Value!);
    }

    [HttpPost("CreateAdmin"), Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> CreateAdmin()
    {
        await _userService.CreateAdminUser();
        return Ok();
    }

    [HttpGet("Current")]
    [ProducesResponseType<UserModel>(StatusCodes.Status200OK)]
    public IActionResult GetUser()
    {
        return Ok(_current.User);
    }

    [HttpGet("ById/{id}")]
    [ProducesResponseType<UserModel>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserById(
        string id,
        CancellationToken stopToken
    ) {
        var user = await _userService.GetUserFromIdAsync(id, stopToken);
        if(user == null) return NotFound();
        else return Ok(user);
    }

    [HttpGet("ByName/{name}")]
    [ProducesResponseType<UserModel>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserByName(
        string name,
        CancellationToken stopToken
    ) {
        var user = await _userService.GetUserFromName(name, stopToken);
        if(user == null) return NotFound();
        else return Ok(user);
    }

    [HttpPost("Create"), Authorize(Role.Admin)]
    [ProducesResponseType<UserModel>(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateUser(
        CreateUserRequest request,
        CancellationToken stopToken
    ) {
        var user = await _userService.CreateUserAsync(request.Name, request.Password, stopToken);
        return Ok(user);
    }

    [HttpDelete("Delete/{userId}"), Authorize(Role.Admin)]
    [ProducesResponseType<UserModel>(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteUser(
        string userId,
        CancellationToken stopToken
    ) {
        var user = await _userService.DeleteUserAsync(userId, stopToken);
        if(user == null) return NotFound();
        else return Ok(user);
    }

    [HttpPut("Update")]
    [ProducesResponseType<UserModel>(StatusCodes.Status200OK)]
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

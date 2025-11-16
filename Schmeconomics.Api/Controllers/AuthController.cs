using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schmeconomics.Api.Auth;

namespace Schmeconomics.Api.Controllers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class AuthController(
    IAuthService _authService
) : ControllerBase
{
    [HttpPost("SignIn")]
    public async Task<IActionResult> SignIn(
        SignInRequest request,
        CancellationToken stopToken = default)
    {
        // Get the IP address from the request
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // Sign in the user
        var authModel = await _authService.SignInAsync(request.Name, request.Password, ipAddress, stopToken);
        
        // Add refresh token to response cookies
        Response.Cookies.Append(
            "refreshToken",
            authModel.RefreshToken,
            new CookieOptions
            {
                // Domain = $"{Request.Scheme}://{Request.Host}",
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = authModel.ExpiresOnUtc,
            });
        
        // Return access token in the response body
        return Ok(new { accessToken = authModel.AccessToken });
    }

    [HttpPost("SignOut")]
    public async Task<IActionResult> SignOut(
        CancellationToken stopToken = default)
    {
        // Get the IP address from the request
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // Get refresh token from cookies
        if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            // Revoke the refresh token
            await _authService.SignOutAsync(refreshToken, ipAddress, stopToken);
            
            // Remove the cookie
            Response.Cookies.Delete("refreshToken");
        }
        
        return Ok();
    }
    
    [HttpPost("Refresh")]
    public async Task<IActionResult> Refresh(CancellationToken stopToken = default)
    {
        // Get the IP address from the request
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // Get refresh token from cookies
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            return BadRequest("Refresh token not found");
        }
        
        try
        {
            // Refresh the token
            var authModel = await _authService.RefreshTokenAsync(ipAddress, refreshToken, stopToken);
            
            // Add refresh token to response cookies (this will be a new refresh token)
            Response.Cookies.Append(
                "refreshToken",
                authModel.RefreshToken,
                new CookieOptions
                {
                    // Domain = $"{Request.Scheme}://{Request.Host}",
                    Path = "/",
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = authModel.ExpiresOnUtc,
                });
            
            // Return access token in the response body
            return Ok(new { accessToken = authModel.AccessToken });
        }
        catch (ArgumentException ex)
        {
            // If refresh token is not found or invalid, return BadRequest
            return BadRequest(ex.Message);
        }
    }
}

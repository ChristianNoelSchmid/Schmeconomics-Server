using Microsoft.AspNetCore.Mvc;
using Schmeconomics.Api.Auth;
using Schmeconomics.Api.Categories;
using Schmeconomics.Api.Users;

namespace Schmeconomics.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Role.Admin)]
public class CategoryController(
    ICategoryService _categoryService,
    ICurrentUser _currentUser
) : ControllerBase
{
    [HttpPost("Create")]
    public async Task<IActionResult> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken stopToken = default)
    {
        if (_currentUser.User == null)
            return Unauthorized();
            
        // Override the AccountId from the request with the one from route to ensure consistency
        var category = await _categoryService.CreateCategoryAsync(
            request.AccountId, 
            request.Name, 
            request.Balance, 
            request.RefillValue, 
            stopToken
        );
        
        if (category.IsOk) return Ok(category.Value);
        else return BadRequest(category.Error.Message);
    }

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> DeleteCategoryAsync(
        string id,
        CancellationToken stopToken = default
    ) {
        if (_currentUser.User == null)
            return Unauthorized();
            
        var result = await _categoryService.DeleteCategoryAsync(id, stopToken);
        
        if (result.IsOk) return Ok();
        else return NotFound(result.Error.Message);
    }

    [HttpPut("Update/{id}")]
    public async Task<IActionResult> UpdateCategoryAsync(
        string id,
        UpdateCategoryRequest request,
        CancellationToken stopToken = default
    ) {
        if (_currentUser.User == null)
            return Unauthorized();
            
        var category = await _categoryService.UpdateCategoryAsync(id, request.Name, request.Balance, request.RefillValue, stopToken);
        
        if (category.IsOk) return Ok(category.Value);
        else return category.Error switch
        {
            CategoryServiceError.CategoryNotFound => NotFound(category.Error.Message),
            CategoryServiceError.CategoryAlreadyExists or _ => BadRequest(category.Error.Message),
        };
    }

    [HttpPut("UpdateOrder")]
    public async Task<IActionResult> UpdateCategoriesOrderAsync(
        UpdateCategoriesOrderRequest request,
        CancellationToken stopToken = default
    ) {
        if (_currentUser.User == null)
            return Unauthorized();
            
        var result = await _categoryService.UpdateCategoryOrdersAsync(request.AccountId, request.CategoryIds, stopToken);
        
        if (result.IsOk) return Ok();
        else return BadRequest(result.Error.Message);
    }
}

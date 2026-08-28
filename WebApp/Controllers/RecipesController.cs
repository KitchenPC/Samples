using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers;

[ApiController]
[Route("api/recipes")]
public sealed class RecipesController(IKitchenPcService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<RecipeListResponse>(StatusCodes.Status200OK)]
    public Task<RecipeListResponse> Search(
        [FromQuery] string? query,
        CancellationToken cancellationToken
    ) => service.SearchRecipesAsync(query, cancellationToken);

    [HttpGet("{id:guid}")]
    [ProducesResponseType<RecipeDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeDetailResponse>> Get(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var recipe = await service.GetRecipeAsync(id, cancellationToken);
        return recipe is null ? NotFound() : Ok(recipe);
    }
}

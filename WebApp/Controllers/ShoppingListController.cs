using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers;

[ApiController]
[Route("api/shopping-list")]
public sealed class ShoppingListController(IKitchenPcService service) : ControllerBase
{
    [HttpPost("aggregate")]
    [ProducesResponseType<ShoppingListResponse>(StatusCodes.Status200OK)]
    public Task<ShoppingListResponse> Aggregate(
        [FromBody] ShoppingListRequest request,
        CancellationToken cancellationToken
    ) => service.BuildShoppingListAsync(request, cancellationToken);
}

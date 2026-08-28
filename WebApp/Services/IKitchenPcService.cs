using WebApp.Models;

namespace WebApp.Services;

public interface IKitchenPcService
{
    Task<RecipeListResponse> SearchRecipesAsync(
        string? query,
        CancellationToken cancellationToken = default
    );

    Task<RecipeDetailResponse?> GetRecipeAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<ShoppingListResponse> BuildShoppingListAsync(
        ShoppingListRequest request,
        CancellationToken cancellationToken = default
    );
}

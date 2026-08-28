namespace WebApp.Models;

public sealed record RecipeListResponse(
    IReadOnlyList<RecipeSummaryResponse> Recipes,
    long TotalCount
);

public sealed record RecipeSummaryResponse(
    Guid Id,
    string Title,
    string? Description,
    string? ImageUrl,
    string? Author,
    short? PrepTime,
    short? CookTime,
    short AverageRating
);

public sealed record RecipeDetailResponse(
    Guid Id,
    string Title,
    string? Description,
    string? ImageUrl,
    string? Author,
    string? Credit,
    string? CreditUrl,
    short? PrepTime,
    short? CookTime,
    short ServingSize,
    short AverageRating,
    IReadOnlyList<string> Tags,
    IReadOnlyList<RecipeIngredientResponse> Ingredients,
    string? Method
);

public sealed record RecipeIngredientResponse(string Text);

public sealed record ShoppingListRequest(
    IReadOnlyList<Guid>? RecipeIds,
    IReadOnlyList<string>? Items
);

public sealed record ShoppingListResponse(
    IReadOnlyList<ShoppingListItemResponse> Items,
    IReadOnlyList<string> UnrecognizedItems
);

public sealed record ShoppingListItemResponse(string Key, string Name, string? Amount);

using KitchenPC.Core;
using KitchenPC.Core.Context;
using KitchenPC.Core.Ingredients;
using KitchenPC.Core.NLP;
using KitchenPC.Core.Recipes;
using KitchenPC.Core.ShoppingLists;
using WebApp.Models;

namespace WebApp.Services;

public sealed class KitchenPcService(DBContext context) : IKitchenPcService
{
    public async Task<RecipeListResponse> SearchRecipesAsync(
        string? query,
        CancellationToken cancellationToken = default
    )
    {
        var results = await context.RecipeSearchAsync(
            new RecipeQuery
            {
                Keywords = Normalize(query),
                Sort = RecipeQuery.SortOrder.Title,
                Direction = RecipeQuery.SortDirection.Ascending,
            },
            cancellationToken
        );

        return new RecipeListResponse(
            results.Briefs.Select(ToSummary).ToArray(),
            results.TotalCount
        );
    }

    public async Task<RecipeDetailResponse?> GetRecipeAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        Recipe? recipe;
        try
        {
            recipe = (
                await context.ReadRecipesAsync(
                    new[] { id },
                    ReadRecipeOptions.MethodOnly,
                    cancellationToken
                )
            ).SingleOrDefault();
        }
        catch (RecipeNotFoundException)
        {
            return null;
        }

        if (recipe is null)
            return null;

        return new RecipeDetailResponse(
            recipe.Id,
            recipe.Title,
            recipe.Description,
            recipe.ImageUrl,
            recipe.OwnerAlias,
            recipe.Credit,
            recipe.CreditUrl,
            recipe.PrepTime,
            recipe.CookTime,
            recipe.ServingSize,
            recipe.AvgRating,
            recipe.Tags?.Select(tag => tag.ToString()).ToArray() ?? Array.Empty<string>(),
            recipe
                .Ingredients.Select(usage => new RecipeIngredientResponse(usage.ToString()))
                .ToArray(),
            HtmlToPlainText.Convert(recipe.Method)
        );
    }

    public Task<ShoppingListResponse> BuildShoppingListAsync(
        ShoppingListRequest request,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var recipeIds = request.RecipeIds?.Distinct().ToArray() ?? Array.Empty<Guid>();
        var rawItems =
            request.Items?.Where(item => !String.IsNullOrWhiteSpace(item)).ToArray()
            ?? Array.Empty<string>();
        var sources = new List<IShoppingListSource>();
        var unrecognized = new List<string>();

        sources.AddRange(context.AggregateRecipes(recipeIds));

        foreach (var rawItem in rawItems)
        {
            var value = rawItem.Trim();
            var result = context.ParseIngredientUsage(value);
            if (result is Match match)
                sources.Add(context.ConvertIngredientUsage(match.Usage));
            else
                unrecognized.Add(value);
        }

        var list = new ShoppingList(null, "Sample shopping list", sources);
        var items = list.Where(item => item.Ingredient is not null)
            .Select(item => new ShoppingListItemResponse(
                item.Ingredient.Id.ToString("D"),
                item.Ingredient.Name,
                item.Amount?.ToString()
            ))
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult(new ShoppingListResponse(items, unrecognized));
    }

    private static RecipeSummaryResponse ToSummary(RecipeBrief recipe) =>
        new(
            recipe.Id,
            recipe.Title,
            recipe.Description,
            recipe.ImageUrl,
            recipe.Author,
            recipe.PrepTime,
            recipe.CookTime,
            recipe.AvgRating
        );

    private static string? Normalize(string? value) =>
        String.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

using Microsoft.AspNetCore.Mvc;
using WebApp.Controllers;
using WebApp.Models;
using WebApp.Services;
using Xunit;

namespace WebApp.Tests;

public sealed class ControllerTests
{
    [Fact]
    public async Task RecipeSearchPassesQueryToService()
    {
        var expected = new RecipeListResponse(Array.Empty<RecipeSummaryResponse>(), 0);
        var service = new FakeKitchenPcService { SearchResult = expected };
        var controller = new RecipesController(service);

        var actual = await controller.Search("  brownies  ", CancellationToken.None);

        Assert.Same(expected, actual);
        Assert.Equal("  brownies  ", service.SearchQuery);
    }

    [Fact]
    public async Task MissingRecipeReturnsNotFound()
    {
        var controller = new RecipesController(new FakeKitchenPcService());

        var response = await controller.Get(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task ShoppingListReturnsAggregatedResult()
    {
        var expected = new ShoppingListResponse(
            new[] { new ShoppingListItemResponse("eggs", "eggs", "12") },
            Array.Empty<string>()
        );
        var service = new FakeKitchenPcService { ShoppingResult = expected };
        var controller = new ShoppingListController(service);
        var request = new ShoppingListRequest(Array.Empty<Guid>(), new[] { "12 eggs" });

        var actual = await controller.Aggregate(request, CancellationToken.None);

        Assert.Same(expected, actual);
        Assert.Same(request, service.ShoppingRequest);
    }

    private sealed class FakeKitchenPcService : IKitchenPcService
    {
        public RecipeListResponse SearchResult { get; set; } =
            new(Array.Empty<RecipeSummaryResponse>(), 0);
        public RecipeDetailResponse? RecipeResult { get; set; }
        public ShoppingListResponse ShoppingResult { get; set; } =
            new(Array.Empty<ShoppingListItemResponse>(), Array.Empty<string>());
        public string? SearchQuery { get; private set; }
        public ShoppingListRequest? ShoppingRequest { get; private set; }

        public Task<RecipeListResponse> SearchRecipesAsync(
            string? query,
            CancellationToken cancellationToken = default
        )
        {
            SearchQuery = query;
            return Task.FromResult(SearchResult);
        }

        public Task<RecipeDetailResponse?> GetRecipeAsync(
            Guid id,
            CancellationToken cancellationToken = default
        ) => Task.FromResult(RecipeResult);

        public Task<ShoppingListResponse> BuildShoppingListAsync(
            ShoppingListRequest request,
            CancellationToken cancellationToken = default
        )
        {
            ShoppingRequest = request;
            return Task.FromResult(ShoppingResult);
        }
    }
}

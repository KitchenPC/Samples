namespace KitchenPC.Samples.ShoppingList.Tests;

[Collection(StaticContextCollection.Name)]
public sealed class ShoppingListServiceTests
{
   private readonly StaticContextFixture fixture;

   public ShoppingListServiceTests(StaticContextFixture fixture)
   {
      this.fixture = fixture;
   }

   [Fact]
   public void RecognizedEntriesAreAggregated()
   {
      using var scope = new TestScope(fixture);

      scope.Service.Add("12 bananas");
      scope.Service.Add("3 bananas");

      var row = Assert.Single(scope.Service.GetRows());
      Assert.True(row.Recognized);
      Assert.Equal("bananas", row.Item);
      Assert.Equal("15", row.Amount);
      Assert.Equal(2, row.EntryCount);
   }

   [Fact]
   public void ConvertibleUnitsAreAggregated()
   {
      using var scope = new TestScope(fixture);

      scope.Service.Add("1 cup milk");
      scope.Service.Add("8 fluid ounces milk");

      var row = Assert.Single(scope.Service.GetRows());
      Assert.Equal("2% milk", row.Item);
      Assert.Equal("2 cups", row.Amount);
   }

   [Fact]
   public void UnrecognizedEntriesRemainVisible()
   {
      using var scope = new TestScope(fixture);

      scope.Service.Add("paper towels");

      var row = Assert.Single(scope.Service.GetRows());
      Assert.False(row.Recognized);
      Assert.Equal("paper towels", row.Item);
      Assert.Empty(row.Amount);
   }

   [Fact]
   public void TogglingAggregatedRowUpdatesEveryContributingEntry()
   {
      using var scope = new TestScope(fixture);
      scope.Service.Add("12 bananas");
      scope.Service.Add("3 bananas");

      scope.Service.TogglePurchased(Assert.Single(scope.Service.GetRows()));

      Assert.All(scope.Service.Document.Entries, entry => Assert.True(entry.Purchased));
      Assert.True(Assert.Single(scope.Service.GetRows()).Purchased);
   }

   [Fact]
   public void RemovingAggregatedRowRemovesEveryContributingEntry()
   {
      using var scope = new TestScope(fixture);
      scope.Service.Add("12 bananas");
      scope.Service.Add("3 bananas");
      scope.Service.Add("a cup of milk");

      var bananas = scope.Service.GetRows().Single(row => row.Item == "bananas");
      scope.Service.Remove(bananas);

      Assert.Single(scope.Service.Document.Entries);
      Assert.Equal("2% milk", Assert.Single(scope.Service.GetRows()).Item);
   }

   [Fact]
   public void PurchasedEntriesCanBeCleared()
   {
      using var scope = new TestScope(fixture);
      scope.Service.Add("12 bananas");
      scope.Service.Add("a cup of milk");
      var bananas = scope.Service.GetRows().Single(row => row.Item == "bananas");
      scope.Service.TogglePurchased(bananas);

      var removed = scope.Service.ClearPurchased();

      Assert.Equal(1, removed);
      Assert.Equal("2% milk", Assert.Single(scope.Service.GetRows()).Item);
   }
}

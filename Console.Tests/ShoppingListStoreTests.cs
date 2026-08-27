namespace KitchenPC.Samples.ShoppingList.Tests;

public sealed class ShoppingListStoreTests
{
   [Fact]
   public void DefaultPathIsAlwaysAbsolute()
   {
      Assert.True(Path.IsPathRooted(ShoppingListStore.GetDefaultPath()));
   }

   [Fact]
   public void MissingFileLoadsAnEmptyDocument()
   {
      using var directory = new TemporaryDirectory();
      var store = new ShoppingListStore(Path.Combine(directory.Path, "list.json"));

      var document = store.Load();

      Assert.Equal(ShoppingListDocument.CurrentVersion, document.Version);
      Assert.Empty(document.Entries);
   }

   [Fact]
   public void DocumentRoundTripsThroughJson()
   {
      using var directory = new TemporaryDirectory();
      var store = new ShoppingListStore(Path.Combine(directory.Path, "nested", "list.json"));
      var id = Guid.NewGuid();
      var document = new ShoppingListDocument
      {
         Entries = [new ShoppingEntry { Id = id, Text = "12 bananas", Purchased = true }],
      };

      store.Save(document);
      var loaded = store.Load();

      var entry = Assert.Single(loaded.Entries);
      Assert.Equal(id, entry.Id);
      Assert.Equal("12 bananas", entry.Text);
      Assert.True(entry.Purchased);
      Assert.False(File.Exists(store.FilePath + ".tmp"));
   }

   [Fact]
   public void InvalidJsonIsNotOverwritten()
   {
      using var directory = new TemporaryDirectory();
      var path = Path.Combine(directory.Path, "list.json");
      const string invalidJson = "{ definitely not JSON";
      File.WriteAllText(path, invalidJson);
      var store = new ShoppingListStore(path);

      var error = Assert.Throws<InvalidDataException>(() => store.Load());

      Assert.Contains("not valid JSON", error.Message);
      Assert.Equal(invalidJson, File.ReadAllText(path));
   }
}

internal sealed class TemporaryDirectory : IDisposable
{
   public TemporaryDirectory()
   {
      Path = System.IO.Path.Combine(
         System.IO.Path.GetTempPath(),
         "kitchenpc-samples-tests",
         Guid.NewGuid().ToString("N")
      );
      Directory.CreateDirectory(Path);
   }

   public string Path { get; }

   public void Dispose()
   {
      if (Directory.Exists(Path))
         Directory.Delete(Path, recursive: true);
   }
}

internal sealed class TestScope : IDisposable
{
   private readonly TemporaryDirectory directory = new();

   public TestScope(StaticContextFixture fixture)
   {
      var store = new ShoppingListStore(Path.Combine(directory.Path, "list.json"));
      Service = new ShoppingListService(fixture.Context, store);
   }

   public ShoppingListService Service { get; }

   public void Dispose() => directory.Dispose();
}

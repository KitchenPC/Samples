using KitchenPC.Core.Context;
using KitchenPC.Core.NLP;

namespace KitchenPC.Samples.ShoppingList;

public sealed class ShoppingListService
{
   private readonly StaticContext context;
   private readonly ShoppingListStore store;

   public ShoppingListService(
      StaticContext context,
      ShoppingListStore store,
      ShoppingListDocument? document = null
   )
   {
      this.context = context ?? throw new ArgumentNullException(nameof(context));
      this.store = store ?? throw new ArgumentNullException(nameof(store));
      Document = document ?? new ShoppingListDocument();
   }

   public ShoppingListDocument Document { get; }
   public string FilePath => store.FilePath;

   public ShoppingEntry Add(string text)
   {
      if (string.IsNullOrWhiteSpace(text))
         throw new ArgumentException("Enter an item before adding it.", nameof(text));

      var entry = new ShoppingEntry { Text = text.Trim() };
      Document.Entries.Add(entry);
      Save();
      return entry;
   }

   public IReadOnlyList<ShoppingListRow> GetRows()
   {
      var parsed = Document.Entries.Select(entry => new
      {
         Entry = entry,
         Result = context.ParseIngredientUsage(entry.Text),
      }).ToList();

      var matches = parsed.Where(item => item.Result is Match).ToList();
      var recognized = matches
         .GroupBy(item => item.Result.Usage.Ingredient.Id)
         .Select(group =>
         {
            var entries = group.Select(item => item.Entry).ToArray();
            var usages = group.Select(item => item.Result.Usage).ToArray();
            var aggregation = context.AggregateIngredients(usages).Single();

            return new ShoppingListRow(
               $"ingredient:{group.Key}",
               entries.All(entry => entry.Purchased),
               aggregation.Amount?.ToString() ?? string.Empty,
               aggregation.Ingredient.Name,
               entries.Length,
               true,
               entries.Select(entry => entry.Id).ToArray()
            );
         });

      var unrecognized = parsed
         .Where(item => item.Result is not Match)
         .Select(item =>
            new ShoppingListRow(
               $"raw:{item.Entry.Id}",
               item.Entry.Purchased,
               string.Empty,
               item.Entry.Text,
               1,
               false,
               [item.Entry.Id]
            )
         );

      return recognized
         .Concat(unrecognized)
         .OrderBy(row => row.Purchased)
         .ThenBy(row => row.Item, StringComparer.OrdinalIgnoreCase)
         .ToArray();
   }

   public void TogglePurchased(ShoppingListRow row)
   {
      ArgumentNullException.ThrowIfNull(row);
      var ids = row.EntryIds.ToHashSet();

      foreach (var entry in Document.Entries.Where(entry => ids.Contains(entry.Id)))
         entry.Purchased = !row.Purchased;

      Save();
   }

   public void Remove(ShoppingListRow row)
   {
      ArgumentNullException.ThrowIfNull(row);
      var ids = row.EntryIds.ToHashSet();
      Document.Entries.RemoveAll(entry => ids.Contains(entry.Id));
      Save();
   }

   public int ClearPurchased()
   {
      var removed = Document.Entries.RemoveAll(entry => entry.Purchased);
      if (removed > 0)
         Save();

      return removed;
   }

   public int ClearAll()
   {
      var removed = Document.Entries.Count;
      if (removed > 0)
      {
         Document.Entries.Clear();
         Save();
      }

      return removed;
   }

   public void Save() => store.Save(Document);
}

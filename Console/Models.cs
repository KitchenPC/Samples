namespace KitchenPC.Samples.ShoppingList;

public sealed class ShoppingListDocument
{
   public const int CurrentVersion = 1;

   public int Version { get; set; } = CurrentVersion;
   public List<ShoppingEntry> Entries { get; set; } = [];
}

public sealed class ShoppingEntry
{
   public Guid Id { get; set; } = Guid.NewGuid();
   public string Text { get; set; } = string.Empty;
   public bool Purchased { get; set; }
}

public sealed record ShoppingListRow(
   string Key,
   bool Purchased,
   string Amount,
   string Item,
   int EntryCount,
   bool Recognized,
   IReadOnlyList<Guid> EntryIds
);

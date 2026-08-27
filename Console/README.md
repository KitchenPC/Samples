# KitchenPC Shopping List TUI

This database-free sample uses [Terminal.Gui](https://github.com/tui-cs/Terminal.Gui) and
KitchenPC `StaticContext` to provide an interactive shopping list in the terminal. Enter natural-
language items and KitchenPC combines compatible quantities automatically.

For example:

```text
12 bananas + 3 bananas             -> bananas: 15
1 cup milk + 8 fluid ounces milk   -> 2% milk: 2 cups
```

Items that the limited sample data cannot recognize, such as `paper towels`, remain in the list as
unrecognized text instead of being discarded.

## Run

From the repository root:

```bash
dotnet run --project Console/Console.csproj
```

The project copies `SampleData/KPCData.xml` into its build output automatically. It does not need a
database or connection string.

## Controls

| Control | Action |
| --- | --- |
| Enter or Add | Add the text-field value |
| Arrow keys or mouse | Select a shopping-list row |
| Space or Enter on a row | Toggle all contributing entries purchased |
| Delete | Remove the selected row after confirmation |
| Ctrl+S | Save immediately |
| F1 | Show keyboard help |
| Ctrl+Q | Quit |

The File menu also provides commands to clear purchased entries or clear the entire list.

## Aggregation behavior

The application retains each original entry and reparses the complete list whenever it changes.
Recognized entries with the same KitchenPC ingredient identity are aggregated using KitchenPC's
unit and ingredient-form conversions. The **Entries** column shows how many original inputs
contributed to an aggregated row.

Removing an aggregated row removes all of its contributing entries. Marking it purchased marks all
of those entries purchased. Adding a new, unpurchased entry to that ingredient makes the aggregate
active again.

## Local storage

The shopping list is saved automatically after each change as versioned JSON under the current
user's local application-data directory:

- Linux: usually `~/.local/share/KitchenPC/Samples/ShoppingList/shopping-list.json`
- Windows: `%LOCALAPPDATA%\KitchenPC\Samples\ShoppingList\shopping-list.json`
- macOS: the .NET local application-data directory under the current user's profile

The application writes a temporary file and replaces the saved list only after serialization
succeeds. Invalid JSON is reported at startup and left untouched for recovery. Delete the JSON file
to reset the sample completely.

The bundled XML snapshot is read-only and is never changed by the application.

## Scope

This example intentionally manages one local shopping list. Multiple named lists, recipe browsing,
inline quantity editing, and database synchronization are possible future extensions.

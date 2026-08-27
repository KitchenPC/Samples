using System.Data;

namespace KitchenPC.Samples.ShoppingList;

public sealed class ShoppingListApp : Runnable
{
   private readonly ShoppingListService service;
   private readonly TextField input;
   private readonly TableView table;
   private readonly Label status;
   private IReadOnlyList<ShoppingListRow> rows = [];

   public ShoppingListApp(ShoppingListService service)
   {
      this.service = service ?? throw new ArgumentNullException(nameof(service));
      Title = "KitchenPC Shopping List";

      var menu = CreateMenu();
      var prompt = new Label { Text = "Add item:", X = 1, Y = Pos.Bottom(menu) + 1 };

      input = new TextField
      {
         X = Pos.Right(prompt) + 1,
         Y = Pos.Top(prompt),
         Width = Dim.Fill(11),
         Text = string.Empty,
      };

      var addButton = new Button
      {
         Title = "_Add",
         X = Pos.AnchorEnd(9),
         Y = Pos.Top(prompt),
         Width = 8,
         IsDefault = true,
      };

      var listFrame = new FrameView
      {
         Title = "Items",
         X = 0,
         Y = Pos.Bottom(input) + 1,
         Width = Dim.Fill(),
         Height = Dim.Fill(2),
      };

      table = new TableView
      {
         Width = Dim.Fill(),
         Height = Dim.Fill(),
         FullRowSelect = true,
         Style = new TableStyle
         {
            ExpandLastColumn = true,
            ShowHorizontalBottomLine = false,
         },
      };

      listFrame.Add(table);

      status = new Label
      {
         X = 1,
         Y = Pos.AnchorEnd(2),
         Width = Dim.Fill(1),
         Text = "Enter an item above. Space toggles purchased; Delete removes a row.",
      };

      var statusBar = new StatusBar(
         [
            new Shortcut(Key.S.WithCtrl, "Save", Save),
            new Shortcut(Key.F1, "Help", ShowHelp),
            new Shortcut(Key.Q.WithCtrl, "Quit", RequestStop),
         ]
      );

      addButton.Accepting += (_, args) =>
      {
         AddItem();
         args.Handled = true;
      };
      table.Accepted += (_, args) =>
      {
         ToggleSelected();
         args.Handled = true;
      };
      table.KeyDown += HandleTableKey;

      Add(menu, prompt, input, addButton, listFrame, status, statusBar);
      RefreshRows();
      input.SetFocus();
   }

   private MenuBar CreateMenu() =>
      new(
         [
            new MenuBarItem(
               "_File",
               [
                  new MenuItem("_Save", "Save the shopping list", Save),
                  new MenuItem(
                     "Clear _purchased",
                     "Remove every purchased entry",
                     ClearPurchased
                  ),
                  new MenuItem("Clear _all", "Remove every entry", ClearAll),
                  new MenuItem("_Quit", "Exit KitchenPC Shopping List", RequestStop),
               ]
            ),
            new MenuBarItem(
               "_Help",
               [
                  new MenuItem("_Keyboard shortcuts", "Show available controls", ShowHelp),
                  new MenuItem("_About", "About this sample", ShowAbout),
               ]
            ),
         ]
      );

   private void AddItem()
   {
      try
      {
         var text = input.Text?.Trim() ?? string.Empty;
         if (string.IsNullOrWhiteSpace(text))
         {
            SetStatus("Enter an item before adding it.");
            input.SetFocus();
            return;
         }

         service.Add(text);
         input.Text = string.Empty;
         RefreshRows();
         SetStatus($"Added “{text}” and saved the list.");
         input.SetFocus();
      }
      catch (ArgumentException exception)
      {
         SetStatus(exception.Message);
      }
      catch (Exception exception)
      {
         ShowError("Could not add item", exception);
      }
   }

   private void HandleTableKey(object? sender, Key key)
   {
      if (key.KeyCode == Key.Space)
      {
         ToggleSelected();
         key.Handled = true;
      }
      else if (key.KeyCode == Key.Delete)
      {
         RemoveSelected();
         key.Handled = true;
      }
   }

   private void ToggleSelected()
   {
      var row = GetSelectedRow();
      if (row is null)
         return;

      try
      {
         service.TogglePurchased(row);
         RefreshRows();
         SetStatus(row.Purchased ? $"Restored {row.Item}." : $"Purchased {row.Item}.");
      }
      catch (Exception exception)
      {
         ShowError("Could not update item", exception);
      }
   }

   private void RemoveSelected()
   {
      var row = GetSelectedRow();
      if (row is null || App is null)
         return;

      var message =
         row.EntryCount == 1
            ? $"Remove “{row.Item}” from the shopping list?"
            : $"Remove “{row.Item}” and its {row.EntryCount} contributing entries?";
      var answer = MessageBox.Query(App, "Remove item", message, "Cancel", "Remove");
      if (answer != 1)
         return;

      try
      {
         service.Remove(row);
         RefreshRows();
         SetStatus($"Removed {row.Item}.");
      }
      catch (Exception exception)
      {
         ShowError("Could not remove item", exception);
      }
   }

   private void ClearPurchased()
   {
      if (App is null)
         return;

      var purchased = service.Document.Entries.Count(entry => entry.Purchased);
      if (purchased == 0)
      {
         SetStatus("There are no purchased items to clear.");
         return;
      }

      var answer = MessageBox.Query(
         App,
         "Clear purchased",
         $"Remove {purchased} purchased entr{(purchased == 1 ? "y" : "ies")}?",
         "Cancel",
         "Clear"
      );
      if (answer != 1)
         return;

      try
      {
         service.ClearPurchased();
         RefreshRows();
         SetStatus("Removed purchased items.");
      }
      catch (Exception exception)
      {
         ShowError("Could not clear purchased items", exception);
      }
   }

   private void ClearAll()
   {
      if (service.Document.Entries.Count == 0 || App is null)
      {
         SetStatus("The shopping list is already empty.");
         return;
      }

      var answer = MessageBox.Query(
         App,
         "Clear shopping list",
         "Remove every item from the shopping list?",
         "Cancel",
         "Clear all"
      );
      if (answer != 1)
         return;

      try
      {
         service.ClearAll();
         RefreshRows();
         SetStatus("Cleared the shopping list.");
      }
      catch (Exception exception)
      {
         ShowError("Could not clear the shopping list", exception);
      }
   }

   private void Save()
   {
      try
      {
         service.Save();
         SetStatus($"Saved to {service.FilePath}");
      }
      catch (Exception exception)
      {
         ShowError("Could not save the shopping list", exception);
      }
   }

   private void RefreshRows()
   {
      var previousIndex = table.Value?.SelectedCell.Y ?? 0;
      rows = service.GetRows();

      var data = new DataTable();
      data.Columns.Add("Bought");
      data.Columns.Add("Amount");
      data.Columns.Add("Entries");
      data.Columns.Add("Item");

      foreach (var row in rows)
      {
         data.Rows.Add(
            row.Purchased ? "✓" : string.Empty,
            row.Amount,
            row.Recognized ? row.EntryCount.ToString() : "unrecognized",
            row.Item
         );
      }

      table.Table = new DataTableSource(data);
      if (rows.Count > 0)
         table.SetSelection(0, Math.Min(previousIndex, rows.Count - 1), false);
      table.Update();
   }

   private ShoppingListRow? GetSelectedRow()
   {
      if (rows.Count == 0)
      {
         SetStatus("The shopping list is empty.");
         return null;
      }

      var selected = table.Value?.SelectedCell.Y ?? 0;
      return selected >= 0 && selected < rows.Count ? rows[selected] : null;
   }

   private void ShowHelp()
   {
      if (App is null)
         return;

      MessageBox.Query(
         App,
         "Keyboard shortcuts",
         "Enter    Add the text-field value\n"
            + "Space    Toggle the selected row purchased\n"
            + "Delete   Remove the selected row\n"
            + "Ctrl+S   Save now\n"
            + "F1       Show this help\n"
            + "Ctrl+Q   Quit",
         "OK"
      );
   }

   private void ShowAbout()
   {
      if (App is null)
         return;

      MessageBox.Query(
         App,
         "About",
         "KitchenPC Shopping List\n\n"
            + "A database-free Terminal.Gui sample that parses and aggregates "
            + "shopping-list entries with KitchenPC StaticContext.",
         "OK"
      );
   }

   private void ShowError(string title, Exception exception)
   {
      SetStatus(exception.Message);
      if (App is not null)
         MessageBox.ErrorQuery(App, title, exception.Message, "OK");
   }

   private void SetStatus(string message) => status.Text = message;
}

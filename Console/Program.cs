using KitchenPC.Core;
using KitchenPC.Core.Context;
using KitchenPC.Samples.ShoppingList;

var dataDirectory = Path.Combine(AppContext.BaseDirectory, "SampleData");
var context = StaticContext
   .Configure.DataDirectory(dataDirectory)
   .Identity(() => AuthIdentity.Anonymous)
   .Create();

try
{
   context.Initialize();

   var store = new ShoppingListStore();
   var document = store.Load();
   var service = new ShoppingListService(context, store, document);

   if (args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase))
   {
      System.Console.WriteLine(
         $"KitchenPC shopping list initialized with {service.GetRows().Count} row(s). "
            + $"Storage: {service.FilePath}"
      );
      return 0;
   }

   using IApplication app = Application.Create().Init();
   using var window = new ShoppingListApp(service) { App = app };
   app.Run(window);
}
catch (Exception exception)
{
   System.Console.Error.WriteLine($"KitchenPC Shopping List could not start: {exception.Message}");
   return 1;
}

return 0;

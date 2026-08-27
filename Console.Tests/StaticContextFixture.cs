using KitchenPC.Core;
using KitchenPC.Core.Context;

namespace KitchenPC.Samples.ShoppingList.Tests;

public sealed class StaticContextFixture
{
   public StaticContextFixture()
   {
      var dataDirectory = Path.Combine(AppContext.BaseDirectory, "SampleData");
      Context = StaticContext
         .Configure.DataDirectory(dataDirectory)
         .Identity(() => AuthIdentity.Anonymous)
         .Create();
      Context.Initialize();
   }

   public StaticContext Context { get; }
}

[CollectionDefinition(Name)]
public sealed class StaticContextCollection : ICollectionFixture<StaticContextFixture>
{
   public const string Name = "StaticContext";
}

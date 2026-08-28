using FluentNHibernate.Cfg.Db;
using KitchenPC.Core;
using KitchenPC.Core.Context;
using KitchenPC.DB;

const string ConnectionStringEnvironmentVariable = "KITCHENPC_CONNECTION_STRING";

try
{
   var options = Options.Parse(args);
   if (options.ShowHelp)
   {
      Options.PrintHelp();
      return 0;
   }

   var connectionString =
      options.ConnectionString
      ?? Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);

   if (string.IsNullOrWhiteSpace(connectionString))
   {
      System.Console.Error.WriteLine(
         $"Set {ConnectionStringEnvironmentVariable} or pass --connection-string."
      );
      Options.PrintHelp();
      return 2;
   }

   var dataDirectory = options.DataDirectory ?? Path.Combine(AppContext.BaseDirectory, "SampleData");
   var dataFile = Path.Combine(dataDirectory, "KPCData.xml");
   if (!File.Exists(dataFile))
      throw new FileNotFoundException("The KitchenPC sample data file was not found.", dataFile);

   if (!options.AssumeYes)
   {
      if (System.Console.IsInputRedirected)
         throw new InvalidOperationException(
            "Provisioning requires confirmation. Run interactively or pass --yes."
         );

      System.Console.WriteLine(
         "WARNING: This recreates the KitchenPC schema and deletes existing KitchenPC data."
      );
      System.Console.Write("Type PROVISION to continue: ");
      if (!string.Equals(System.Console.ReadLine(), "PROVISION", StringComparison.Ordinal))
      {
         System.Console.WriteLine("Provisioning cancelled.");
         return 1;
      }
   }

   var source = StaticContext
      .Configure.DataDirectory(dataDirectory)
      .Identity(() => AuthIdentity.Anonymous)
      .Create();

   var destination = DBContext
      .Configure.Adapter(
         DatabaseAdapter.Configure.DatabaseConfiguration(
            PostgreSQLConfiguration.PostgreSQL82.ConnectionString(connectionString)
         )
      )
      .Identity(() => AuthIdentity.Anonymous)
      .Create();

   try
   {
      System.Console.WriteLine($"Loading sample data from {dataFile}...");
      source.Initialize();
      var data = source.Export();
      System.Console.WriteLine(
         $"Loaded {data.Ingredients.Length:N0} ingredients and {data.Recipes.Count:N0} recipes."
      );

      System.Console.WriteLine("Creating the KitchenPC database schema...");
      destination.InitializeStore();

      System.Console.WriteLine("Importing the sample data...");
      destination.Import(source);

      System.Console.WriteLine("KitchenPC database provisioning completed successfully.");
   }
   finally
   {
      (destination.Adapter as IDisposable)?.Dispose();
   }

   return 0;
}
catch (ArgumentException exception)
{
   System.Console.Error.WriteLine($"Invalid arguments: {exception.Message}");
   Options.PrintHelp();
   return 2;
}
catch (Exception exception)
{
   System.Console.Error.WriteLine($"Database provisioning failed: {exception.Message}");
   return 1;
}

internal sealed record Options(
   string? ConnectionString,
   string? DataDirectory,
   bool AssumeYes,
   bool ShowHelp
)
{
   public static Options Parse(string[] args)
   {
      string? connectionString = null;
      string? dataDirectory = null;
      var assumeYes = false;
      var showHelp = false;

      for (var index = 0; index < args.Length; index++)
      {
         switch (args[index])
         {
            case "--connection-string":
               connectionString = ReadValue(args, ref index, "--connection-string");
               break;
            case "--data-directory":
               dataDirectory = Path.GetFullPath(ReadValue(args, ref index, "--data-directory"));
               break;
            case "--yes":
               assumeYes = true;
               break;
            case "--help":
            case "-h":
               showHelp = true;
               break;
            default:
               throw new ArgumentException($"Unknown option '{args[index]}'.");
         }
      }

      return new Options(connectionString, dataDirectory, assumeYes, showHelp);
   }

   public static void PrintHelp()
   {
      System.Console.WriteLine(
         """
         KitchenPC database initializer

         Usage:
           dotnet run --project DatabaseInitializer/DatabaseInitializer.csproj -- [options]

         Options:
           --connection-string <value>  PostgreSQL connection string. Prefer the
                                        KITCHENPC_CONNECTION_STRING environment variable.
           --data-directory <path>      Directory containing KPCData.xml. The bundled
                                        SampleData directory is used by default.
           --yes                        Skip the destructive-operation confirmation.
           -h, --help                   Show this help.
         """
      );
   }

   private static string ReadValue(string[] args, ref int index, string option)
   {
      if (++index >= args.Length || string.IsNullOrWhiteSpace(args[index]))
         throw new ArgumentException($"{option} requires a value.");

      return args[index];
   }
}

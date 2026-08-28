using FluentNHibernate.Cfg.Db;
using KitchenPC.Core;
using KitchenPC.Core.Context;
using KitchenPC.Core.Middleware;
using KitchenPC.DB;
using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString =
    builder.Configuration.GetConnectionString("KPCContext")
    ?? throw new InvalidOperationException(
        "Connection string 'KPCContext' is missing. See WebApp/README.md for setup instructions."
    );

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddKPCContext(
    Configuration<DBContext>
        .Build.Context(
            DBContext
                .Configure.Adapter(
                    DatabaseAdapter
                        .Configure.DatabaseConfiguration(
                            PostgreSQLConfiguration.PostgreSQL82.ConnectionString(connectionString)
                        )
                        .SearchProvider(NHSearch.Instance)
                )
                .Capabilities(DBContextCapabilities.IngredientParsing)
                .Identity(() => AuthIdentity.Anonymous)
        )
        .Create()
);
builder.Services.AddScoped<IKitchenPcService, KitchenPcService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }

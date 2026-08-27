# KitchenPC Web API Sample

This sample demonstrates using KitchenPC from an ASP.NET Core Web API. It configures a PostgreSQL-
backed `DBContext`, registers it with dependency injection, and exposes an endpoint that parses an
ingredient name.

## Prerequisites

- .NET 10 SDK
- A PostgreSQL database containing the KitchenPC schema and data

Database provisioning is not yet automated. More detailed provisioning guidance or tooling will
be added separately. If you do not already have a KitchenPC database, start with the database-free
[console sample](../Console/README.md).

## Configure the connection

From the repository root, store the connection string with .NET user secrets:

```bash
dotnet user-secrets set \
  --project WebApp/WebApp.csproj \
  "ConnectionStrings:KPCContext" \
  "Host=localhost;Port=5432;Database=kitchenpc;Username=postgres;Password=your-password"
```

User secrets keep local credentials out of `appsettings.json` and source control. Environment-
specific configuration or a secret manager should be used for deployed applications.

## Run the API

```bash
dotnet run --project WebApp/WebApp.csproj
```

In the Development environment, Swagger UI opens at `https://localhost:5001/swagger`. The first
startup initializes KitchenPC's in-memory parsing and modeling indexes from the database, so it may
take longer than a typical small API.

Try the endpoint from Swagger, or request it directly:

```text
GET /Ingredient?ing=carrots
```

A recognized ingredient returns its normalized identifier and conversion information. An unknown
ingredient returns HTTP 404.

## How it works

- `Startup.ConfigureServices` builds a PostgreSQL `DatabaseAdapter`.
- `AddKPCContext` initializes and registers a `DBContext` for dependency injection.
- `IngredientController` receives that context and calls `ParseIngredient`.

The application uses an anonymous KitchenPC identity because this sample does not implement user
authentication.

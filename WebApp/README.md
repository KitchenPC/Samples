# KitchenPC React Web Sample

This sample is a small, public recipe application built with ASP.NET Core, React, TypeScript, and
PostgreSQL. It is intentionally more complete than a single API controller while remaining small
enough to use as the starting point for another KitchenPC website.

Users can browse and search recipes, view recipe details, add recipe ingredients to an aggregated
shopping list, and type additions such as `12 eggs` or `a cup of milk` using KitchenPC's natural-
language parser. Checked items and list sources persist in browser local storage.

There are no accounts, authentication, menus, queues, or recipe-modeling screens. Shopping-list
source data stays in the browser and is sent to the API for KitchenPC parsing and aggregation; it
is not stored in PostgreSQL.

Recipes with photographs use full-size images from the public `images.kitchenpc.com` CDN. Recipes
without a production photograph use the sample application's local styled placeholder, so the UI
remains usable when an image is unavailable. This means photographs require an internet connection
even when the sample database and application are running locally.

## Architecture

- `Program.cs` configures the API and a PostgreSQL-backed `DBContext`.
- `Controllers/` contains the recipe and shopping-list HTTP endpoints.
- `Services/KitchenPcService.cs` keeps KitchenPC domain logic out of the controllers.
- `ClientApp/` is a React and TypeScript application built with Vite.
- ASP.NET Core serves the production frontend from `wwwroot`.

The `DBContext` enables only `IngredientParsing`. Search and recipe details remain database-backed,
while `AggregateRecipes` uses Core's database fallback. This avoids loading autocomplete and
recipe-modeling graphs that the application does not use.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) 22 or later
- PostgreSQL with the KitchenPC sample schema and data

## 1. Create the sample database

Follow [DatabaseInitializer/README.md](../DatabaseInitializer/README.md) to start PostgreSQL,
create `KPCSample`, and import `SampleData/KPCData.xml`. The documented defaults are:

```text
Host=localhost;Port=5432;Database=KPCSample;Username=postgres;Password=postgres
```

## 2. Install and build the React client

```bash
cd WebApp/ClientApp
npm ci
npm run build
cd ../..
```

The development connection string in `appsettings.Development.json` matches the Database
Initializer. To use another connection, set a user secret:

```bash
dotnet user-secrets set \
  --project WebApp/WebApp.csproj \
  "ConnectionStrings:KPCContext" \
  "Host=localhost;Port=5432;Database=KPCSample;Username=postgres;Password=postgres"
```

## 3. Run the application

Run the built frontend through ASP.NET Core:

```bash
dotnet run --project WebApp/WebApp.csproj
```

Open `http://localhost:5000`. The first startup initializes only KitchenPC's NLP indexes.

For frontend hot reload, run the API and Vite in separate terminals:

```bash
dotnet run --project WebApp/WebApp.csproj --no-launch-profile --urls http://localhost:5000
```

```bash
cd WebApp/ClientApp
npm run dev
```

Open `http://localhost:5173`; Vite proxies `/api` requests to port 5000.

## API endpoints

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `GET` | `/api/recipes` | Browse recipes |
| `GET` | `/api/recipes?query=brownies` | Search recipes |
| `GET` | `/api/recipes/{id}` | Load recipe details |
| `POST` | `/api/shopping-list/aggregate` | Aggregate recipes and free-form items |

Example aggregation request:

```json
{
  "recipeIds": ["8daca8ea-baf1-44ad-94b6-70eff0eea3c9"],
  "items": ["12 eggs", "a cup of milk"]
}
```

## Tests and checks

```bash
dotnet test WebApp.Tests/WebApp.Tests.csproj
cd WebApp/ClientApp
npm run build
npm run lint
npm test
```

The backend controller tests do not require PostgreSQL. Running the application and exercising the
API requires the provisioned database.

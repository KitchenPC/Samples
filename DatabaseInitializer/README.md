# KitchenPC Database Initializer

This sample creates the KitchenPC schema in an empty PostgreSQL database and imports the bundled
`SampleData/KPCData.xml` snapshot. The result contains the ingredient catalog, ingredient forms,
natural-language parsing data, and a small collection of recipes needed by the Web API sample.

The tool demonstrates database provisioning with two KitchenPC contexts:

1. A `StaticContext` loads `KPCData.xml` into memory.
2. A `DBContext` connects to PostgreSQL.
3. `DBContext.InitializeStore()` creates the KitchenPC schema.
4. `DBContext.Import(staticContext)` copies the static data into PostgreSQL.

> **Warning:** `InitializeStore()` recreates the KitchenPC schema. Running this tool against an
> existing KitchenPC database deletes its existing KitchenPC tables and data. Use a dedicated,
> newly created sample database.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://docs.docker.com/get-docker/)
- A PostgreSQL client (`psql`), either locally installed or run inside the container

## 1. Start PostgreSQL

The following starts PostgreSQL 17 in a Docker container and exposes it on local port 5432:

```bash
docker run --name kitchenpc-postgres \
  --detach \
  --env POSTGRES_PASSWORD=postgres \
  --publish 5432:5432 \
  postgres:17
```

If the container already exists but is stopped, restart it instead:

```bash
docker start kitchenpc-postgres
```

## 2. Create an empty database

Run `psql` in the container to create the blank `KPCSample` database:

```bash
docker exec -it kitchenpc-postgres \
  psql --username postgres --command 'CREATE DATABASE "KPCSample";'
```

The initializer creates tables, not the database itself. If `KPCSample` already exists and you
want a clean start, drop and recreate it explicitly before continuing.

## 3. Configure the connection

From the Samples repository root, put the development connection string in an environment
variable. This avoids placing the password in source control or passing it in shell history:

```bash
export KITCHENPC_CONNECTION_STRING='Host=localhost;Port=5432;Database=KPCSample;Username=postgres;Password=postgres'
```

Use the hostname, published port, username, and password appropriate for your PostgreSQL setup.

## 4. Initialize and populate KitchenPC

Run the initializer:

```bash
dotnet run --project DatabaseInitializer/DatabaseInitializer.csproj
```

The program displays a destructive-operation warning. Type `PROVISION` to proceed. For an
unattended development setup, pass `--yes` after the `--` argument separator:

```bash
dotnet run --project DatabaseInitializer/DatabaseInitializer.csproj -- --yes
```

Successful output reports how many ingredients and recipes were loaded, creates the schema, and
imports the sample data. Rerunning the initializer recreates the schema and replaces all data.

## 5. Verify the database

List the KitchenPC tables:

```bash
docker exec -it kitchenpc-postgres \
  psql --username postgres --dbname KPCSample --command '\dt'
```

Check the imported row counts:

```bash
docker exec -it kitchenpc-postgres \
  psql --username postgres --dbname KPCSample \
  --command 'SELECT COUNT(*) AS ingredients FROM shoppingingredients; SELECT COUNT(*) AS recipes FROM recipes;'
```

`shoppingingredients` is the legacy KitchenPC schema name for the main ingredient catalog.

## 6. Run the Web API sample

Give the Web API the same connection string through .NET user secrets:

```bash
dotnet user-secrets set \
  --project WebApp/WebApp.csproj \
  "ConnectionStrings:KPCContext" \
  "$KITCHENPC_CONNECTION_STRING"
```

Then run it:

```bash
dotnet run --project WebApp/WebApp.csproj
```

See [the WebApp README](../WebApp/README.md) for its Swagger endpoint and usage details.

## Options

```text
--connection-string <value>  Override KITCHENPC_CONNECTION_STRING.
--data-directory <path>      Use another directory containing KPCData.xml.
--yes                        Skip interactive confirmation.
-h, --help                   Display command help.
```

Prefer the environment variable over `--connection-string`, because command-line arguments can
be visible in process listings and shell history.

# KitchenPC Samples

Example applications that demonstrate how to build on the
[KitchenPC recipe engine](https://github.com/KitchenPC/core).

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

Restore and build every sample from the repository root:

```bash
dotnet restore Samples.slnx
dotnet build Samples.slnx --configuration Release --no-restore
```

## Included samples

### Shopping List TUI

A database-free Terminal.Gui application that initializes `StaticContext` from the bundled sample
data, parses natural-language shopping items, aggregates compatible quantities, and persists the
list in the current user's local application-data directory.

See [Console/README.md](Console/README.md) for details.

### WebApp

An ASP.NET Core Web API that uses `DBContext` with PostgreSQL and exposes an ingredient-parsing
endpoint through Swagger.

See [WebApp/README.md](WebApp/README.md) for setup instructions.

## Sample data

`SampleData/KPCData.xml` is a small static snapshot containing ingredients, forms, NLP data, and
a limited collection of recipes. It is intended for examples and local experimentation; it is not
a replacement for a fully provisioned KitchenPC database.

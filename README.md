# Product Management API

A small REST API for a product catalogue. It covers CRUD, stock movements, search by name and queries by stock range. Product IDs are 6-digit numbers handed out by the database, so they stay unique even with several API instances writing at once.

Built with .NET 10, ASP.NET Core Minimal APIs, EF Core 10 and SQL Server 2022. For local development everything runs under .NET Aspire. Tests use xUnit v3, NSubstitute, Testcontainers and Reqnroll. Scalar is the API explorer.

## What you need

- .NET 10 SDK (the version is pinned in `global.json` with `rollForward: latestFeature`)
- Docker, running

The first build downloads the Aspire CLI/dashboard bundle (`AspireUseCliBundle=true` in the AppHost project), so it needs internet access once. You don't need a dev certificate. The AppHost's `http` profile sets `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true`.

## Running it

```bash
dotnet run --project src/ProductManagement.AppHost
```

This starts a throwaway SQL Server container. `ProductManagement.MigrationService` then applies the migrations and seeds 24 products. Once it finishes, the API starts with two replicas behind one endpoint.

- Dashboard: http://localhost:15009. Use the link with `?t=<token>` that the console prints, because the dashboard asks for a login token.
- API: http://localhost:5080/api/products (nothing is mapped at `/`, so the root returns a 404)
- Scalar: http://localhost:5080/scalar (Development only)
- OpenAPI document: http://localhost:5080/openapi/v1.json (Development only)

When you stop the AppHost with Ctrl+C, the container is removed and nothing is kept. Every run starts again from the same 24 seeded products.

On the first run Aspire may offer to trust the ASP.NET Core HTTPS dev certificate. The app only serves HTTP, so you can safely decline.

The connection strings use `TrustServerCertificate=True` because the local container uses a self-signed certificate. That's fine locally. A real deployment should use a proper certificate and `Encrypt=True`.

### Without Aspire

Start SQL Server yourself:

```bash
export MSSQL_SA_PASSWORD='<pick a strong password>'
docker run -d --name products-sql -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD="$MSSQL_SA_PASSWORD" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

If port 1433 is already in use, map a different host port (e.g. `-p 11433:1433`) and change the connection string to match.

Then run the migration service followed by the API:

```bash
export ConnectionStrings__productsdb="Server=localhost,1433;Database=productsdb;User ID=sa;Password=$MSSQL_SA_PASSWORD;TrustServerCertificate=True"
dotnet run --project src/ProductManagement.MigrationService
dotnet run --project src/ProductManagement.Api --launch-profile http
```

The migration service applies pending migrations, seeds the catalogue if it's empty, and exits. The API then listens on http://localhost:5080. The `http` profile runs in Development, so Scalar, OpenAPI, `/health` and `/alive` are all available.

You can also apply migrations with `dotnet ef`, which seeds as well:

```bash
dotnet tool restore
dotnet ef database update --project src/ProductManagement.Infrastructure --startup-project src/ProductManagement.Infrastructure --connection "$ConnectionStrings__productsdb"
```

## Trying it out

The easiest way is Scalar. If you use Rider or IntelliJ, [`src/ProductManagement.Api/Products.http`](src/ProductManagement.Api/Products.http) exercises every endpoint, both happy paths and error cases. Run it top to bottom. Each request checks its status code, and later requests reuse the IDs and `rowVersion` values captured earlier. SKUs are generated on every run, so you can run it as many times as you like. Other editors ignore the response scripts.

Or with curl:

```bash
curl -X POST http://localhost:5080/api/products \
  -H "Content-Type: application/json" \
  -d '{"sku":"KB-MX-001","name":"Mechanical Keyboard","price":49.99,"stock":12}'
# 201, Location: /api/products/<id>

curl -X POST http://localhost:5080/api/products \
  -H "Content-Type: application/json" \
  -d '{"sku":"bad sku!","name":"","price":0,"stock":-1}'
# 400, one error per invalid field
```

## Endpoints

| Method | Route | Success | Errors |
|---|---|---|---|
| GET | `/api/products` | 200 | |
| POST | `/api/products` | 201 + `Location` | 400, 409 duplicate SKU, 500 ID space exhausted |
| GET | `/api/products/{id}` | 200 | 404 |
| PUT | `/api/products/{id}` | 200 with the new `rowVersion` | 400, 404, 409 concurrency conflict |
| DELETE | `/api/products/{id}` | 204 | 404 |
| POST | `/api/products/{id}/decrement-stock/{quantity}` | 200 | 400, 404, 409 insufficient stock |
| POST | `/api/products/{id}/add-to-stock/{quantity}` | 200 | 400, 404, 409 stock overflow |
| GET | `/api/products/search?name={name}` | 200 | 400 if `name` is missing, blank or too long |
| GET | `/api/products/stock-level?min={min}&max={max}` | 200 | 400 if a bound is missing or `min > max` |

`{quantity}` is a plain route segment on purpose, not `{quantity:int}`. That way `.../decrement-stock/abc` returns a 400 binding error instead of a confusing 404.

## Validation rules

| Field | Rule |
|---|---|
| `sku` | Required, `^[A-Z0-9-]{3,32}$`, unique, can't be changed after creation |
| `name` | Required, 1–100 characters |
| `description` | Optional, max 500 characters |
| `price` | Required, 0.01–1,000,000.00, at most 2 decimal places |
| `stock` | ≥ 0 on create. `PUT` can't change it; use the stock endpoints |
| `rowVersion` | Required on `PUT` and must match the current row, otherwise 409 |
| `quantity` | Integer ≥ 1 |

## Tests

```bash
dotnet test --project tests/ProductManagement.UnitTests/ProductManagement.UnitTests.csproj
dotnet test --project tests/ProductManagement.IntegrationTests/ProductManagement.IntegrationTests.csproj
dotnet test --project tests/ProductManagement.AcceptanceTests/ProductManagement.AcceptanceTests.csproj
```

- **Unit tests** don't need Docker. They cover the `Product` invariants, `ProductService` with the repository substituted, request validation and the shape of the seed data.
- **Integration tests** send real HTTP requests to a real SQL Server container started by Testcontainers. Two of them matter most. `IdGenerationTests` fires 50 concurrent creates across two API instances and checks that no ID is duplicated. `ConcurrencyTests` sends more decrements than there is stock and checks that exactly the right number succeed.
- **Acceptance tests** are Gherkin scenarios in [`tests/ProductManagement.AcceptanceTests/Features`](tests/ProductManagement.AcceptanceTests/Features). They run against the same container setup.

All three suites use Microsoft.Testing.Platform (set in `global.json`), which is why they're run with `dotnet test --project`.

Code style is enforced at build time. `Directory.Build.props` turns on `TreatWarningsAsErrors` and `EnforceCodeStyleInBuild`, and `.editorconfig` holds the rules. To check formatting without building:

```bash
dotnet format ProductManagement.sln --verify-no-changes --severity warn
```

Package versions live in `Directory.Packages.props`.

## CI

[`.github/workflows/ci.yml`](.github/workflows/ci.yml) runs on pull requests and on pushes to `main`. The first job restores, builds with warnings as errors, checks formatting, checks that the EF Core migrations match the model, and runs the unit tests. After that, the integration and acceptance suites run in parallel against SQL Server containers. Each suite publishes a test report to the PR checks and uploads its TRX files as an artifact, kept for 7 days.

One catch: PRs from forks get a read-only token, so the report step fails there.

You can also run the workflow locally with [act](https://github.com/nektos/act). It works on Windows, macOS and Linux, as long as Docker is running Linux containers (on Windows that means Docker Desktop with WSL2, not Windows containers mode). Use the standalone binary (`winget install nektos.act`, `brew install act`) or the GitHub CLI extension (`gh extension install nektos/gh-act`):

```bash
act push -P ubuntu-latest=catthehacker/ubuntu:act-latest
# or
gh act push -P ubuntu-latest=catthehacker/ubuntu:act-latest
```

On Docker Desktop (e.g. under WSL), also pass `--env TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal`, otherwise Testcontainers can't reach the SQL containers from inside the job container. The report and upload steps need GitHub's services, so expect them to fail under act.

## Layout

```text
src/
  ProductManagement.Domain/            Product entity and its rules, no dependencies
  ProductManagement.Application/       ProductService, commands, DTOs, Result, IProductRepository
  ProductManagement.Infrastructure/    DbContext, migrations, repository, seeding
  ProductManagement.Api/               Endpoints, request DTOs, problem-details mapping
  ProductManagement.MigrationService/  Applies migrations and seeds, then exits
  ProductManagement.AppHost/           Aspire wiring: SQL container, migrations, API
  ProductManagement.ServiceDefaults/   OpenTelemetry and health checks

tests/
  ProductManagement.UnitTests/
  ProductManagement.IntegrationTests/
  ProductManagement.AcceptanceTests/
  ProductManagement.Testing/           Shared WebApplicationFactory and SQL Server fixture
```

Domain depends on nothing. Application depends only on Domain, and Infrastructure and Api depend on Application. Api also references Infrastructure, but only to register services in `Program.cs`. `ProductService` never touches EF Core.

## Why it's built this way

**IDs come from a SQL Server sequence.** `ProductIds` runs from 100000 to 999999 with `NO CYCLE`, and each insert takes `NEXT VALUE FOR`, so instances never need to coordinate. I rejected three alternatives:

- Random IDs need collision retries, and the retries get worse as the range fills up.
- HiLo loses whole blocks of IDs whenever an instance restarts, and with only 900k IDs that adds up.
- `IDENTITY` has no natural 6-digit ceiling.

With the sequence, running out raises SQL error 11728. `ProductRepository.TryTranslate` turns that into a 500 "Product ID space exhausted".

**Stock changes are a single conditional UPDATE.** Decrement runs `UPDATE ... WHERE Id = @id AND Stock >= @quantity`, and add checks that it won't overflow `int`. The database enforces the rule in one round trip. Loading the row, checking it in memory and saving would race under load, which is exactly what `ConcurrencyTests` covers.

**`PUT` uses `rowversion` for optimistic concurrency.** The client sends back the `rowVersion` it last saw, and a stale value gets a 409. `rowversion` changes on every write, including stock changes, so a `PUT` that races a stock movement also gets a 409 even though the two touch different columns. That's stricter than it needs to be, but it's safe.

**Migrations run in their own service.** The API waits for `MigrationService` to complete before starting. This keeps migrations out of API startup and stops the two replicas from racing to apply the same migration.

**Some libraries are deliberately left out.** AutoMapper, MediatR and FluentAssertions all moved to commercial licences. Mapping is a two-line `ToDto()` extension, the service calls the repository directly, and tests use xUnit's `Assert`. Moq is still free, but after the 2023 SponsorLink episode I'd rather not depend on it. NSubstitute covers the one fake the tests need.

## Known limitations

- At most 900,000 products can ever be created, and deleted IDs aren't reused. `IdGenerationTests` checks what happens when the range runs out.
- There's no authentication. Anyone who can reach the API can change the catalogue.
- There's no pagination on the list, search or stock-level endpoints. That's fine at this size, but it would need adding before the catalogue grows much.
- As described above, a `PUT` racing a stock change gets a 409 even when nothing actually conflicts.

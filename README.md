# Property Aggregator – Backend

.NET backend of a real-estate offers aggregator. A separate Python scraper
collects listings from property portals and publishes them to RabbitMQ. This backend receives
and stores the offers and exposes them through an HTTP API.

> **Status:** early development

## Architecture

```
 Python scraper ──► RabbitMQ ──► Worker ──► PostgreSQL (PostGIS) ◄── Api ◄── clients
```

| Project | Role |
|---|---|
| `PropertyAggregator.Api` | HTTP API, OpenAPI document and Scalar UI |
| `PropertyAggregator.Worker` | Background service (will consume offers from RabbitMQ) |
| `PropertyAggregator.Application` | Use cases and message contracts |
| `PropertyAggregator.Domain` | Domain model |
| `PropertyAggregator.Infrastructure` | Database, messaging, health checks, environment access |
| `PropertyAggregator.ServiceDefaults` | Shared defaults: OpenTelemetry, health endpoint, HTTP resilience |
| `tests/` | Unit and integration tests |

## Requirements

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/products/docker-desktop/) (PostgreSQL and RabbitMQ run in containers)
- The scraper repository, which provides RabbitMQ in its `docker-compose.yml`

## Running locally

1. Start PostgreSQL with PostGIS:

   ```bash
   docker compose up -d
   ```

2. Start RabbitMQ from the scraper repository:

   ```bash
   docker compose up -d rabbitmq
   ```

3. Start the backend: run `PropertyAggregator.Api` and `PropertyAggregator.Worker`
   from the IDE (a compound run configuration starts both), or in two terminals:

   ```bash
   dotnet run --project src/PropertyAggregator.Api
   dotnet run --project src/PropertyAggregator.Worker
   ```

### Useful addresses

| Address | What |
|---|---|
| `http://localhost:5209/` | Redirects to the API documentation (Development only) |
| `http://localhost:5209/scalar/v1` | API documentation (Scalar) |
| `http://localhost:5209/openapi/v1.json` | OpenAPI document |
| `http://localhost:5209/health` | Health check, returns `Healthy` or `Unhealthy` (503) |

## Database migrations

Migrations are stored in `src/PropertyAggregator.Infrastructure/Persistence/Migrations`
and committed to the repository. Restore the EF Core CLI once with `dotnet tool restore`.

`dotnet ef` does not read `launchSettings.json`, so set the connection string in the
shell first (same value as in `docker-compose.yml`).
The variable lasts only for the current terminal session.

PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=property_aggregator;Username=postgres;Password=postgres"
```

Apply migrations (PostgreSQL has to be running):

```bash
dotnet ef database update --project src/PropertyAggregator.Infrastructure --startup-project src/PropertyAggregator.Api
```

After changing the model, add a migration, apply it, and commit it together with the change:

```bash
dotnet ef migrations add <MigrationName> --project src/PropertyAggregator.Infrastructure --startup-project src/PropertyAggregator.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/PropertyAggregator.Infrastructure --startup-project src/PropertyAggregator.Api
```

## Configuration

The application reads its settings from environment variables through
`EnvironmentService`. A missing or empty variable fails with a clear error.

| Variable | Used by | Description |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Api, Worker | PostgreSQL connection string |
| `RABBITMQ_HOST` | Worker | RabbitMQ host |
| `RABBITMQ_PORT` | Worker | RabbitMQ port (positive integer) |
| `RABBITMQ_USERNAME` | Worker | RabbitMQ user |
| `RABBITMQ_PASSWORD` | Worker | RabbitMQ password |
| `RABBITMQ_QUEUE` | Worker | Queue with offers (`offers`) |
| `RABBITMQ_EXCHANGE` | Worker | Exchange the scraper publishes to (`property_scraper`) |

RabbitMQ variable names match the scraper's, so the values can be the same on both sides.

Local values are set in each project's `Properties/launchSettings.json` and match
`docker-compose.yml` (`postgres` / `postgres`, database `property_aggregator`).
They are credentials for the local containers only.

## Messaging contract

The scraper declares a durable `direct` exchange (`property_scraper`) and a durable
queue (`offers`), bound with two routing keys. Messages are persistent JSON (UTF-8)
with camelCase property names.

| Routing key | Message |
|---|---|
| `offer` | A single offer: `portal`, `sourceId`, `sourceUrl`, `title`, `price`, `pricePerM2`, `areaM2`, `rooms`, `floor`, `street`, `district`, `city`, `province`, `imageUrl`, `transactionType`, `propertyType`, `marketType`, `sellerType`, `scrapedAt` |
| `scrape_event` | End of a scrape run: `event`, `portal`, `scrapeType`, `totalOffers`, `completed` |

An offer is identified by `portal` + `sourceId`. The same offer is published again
on every scrape run, so storing it has to be idempotent.


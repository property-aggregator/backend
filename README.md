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
| `PropertyAggregator.ServiceDefaults` | Shared Aspire defaults: OpenTelemetry, health endpoint, HTTP resilience |
| `PropertyAggregator.AppHost` | .NET Aspire host that starts Api and Worker locally |
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

3. Start the backend through Aspire:

   ```bash
   dotnet run --project src/PropertyAggregator.AppHost
   ```

   The Aspire dashboard opens in the browser (`http://localhost:15032`). It lists
   `api` and `worker` with their state and logs.

   Alternatively, run a single project from the IDE or with
   `dotnet run --project src/PropertyAggregator.Api`.

### Useful addresses

| Address | What |
|---|---|
| `http://localhost:5209/` | Redirects to the API documentation (Development only) |
| `http://localhost:5209/scalar/v1` | API documentation (Scalar) |
| `http://localhost:5209/openapi/v1.json` | OpenAPI document |
| `http://localhost:5209/health` | Health check, returns `Healthy` or `Unhealthy` (503) |
| `http://localhost:15032` | Aspire dashboard |

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


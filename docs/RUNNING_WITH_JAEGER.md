# Running with OpenTelemetry and Jaeger

This guide shows how to run the E-Commerce Backend with full observability using OpenTelemetry and Jaeger for distributed tracing.

## Quick Start with Docker Compose

### Production Mode (Port 80)

#### 1. Start all services

```powershell
docker-compose up -d
```

This will start:

-   **PostgreSQL**: Database on port 5432
-   **Jaeger**: Distributed tracing on port 16686 (UI) and 4317 (OTLP)
-   **E-Commerce Backend**: API on port 80 (production mode)

#### 2. Access the services

-   **API Documentation**: http://localhost/docs
-   **Jaeger UI**: http://localhost:16686
-   **API Health**: http://localhost/api/v1/health (if implemented)

### Development Mode (Port 5049)

#### 1. Start all services in development mode

```powershell
docker-compose -f docker-compose.dev.yml up -d
```

This will start:

-   **PostgreSQL**: Database on port 5432 (with `ecommerce` database)
-   **Jaeger**: Distributed tracing on port 16686 (UI) and 4317 (OTLP)
-   **E-Commerce Backend**: API on port 5049 (development mode with console exporter)

#### 2. Access the services

-   **API Documentation**: http://localhost:5049/docs
-   **Jaeger UI**: http://localhost:16686
-   **API Health**: http://localhost:5049/api/v1/health (if implemented)

### 3. View traces in Jaeger

### 3. View traces in Jaeger

1. Open http://localhost:16686
2. Select **ECommerce.Backend** (or **ECommerce.Backend.Dev** in development mode) from the Service dropdown
3. Click **Find Traces**
4. Click on any trace to see detailed spans

### 4. Stop all services

Production mode:

```powershell
docker-compose down
```

Development mode:

```powershell
docker-compose -f docker-compose.dev.yml down
```

To also remove volumes (database data):

```powershell
# Production
docker-compose down -v

# Development
docker-compose -f docker-compose.dev.yml down -v
```

## Running Locally with Jaeger

If you want to run the API locally but use Jaeger for traces:

### 1. Start only Jaeger and PostgreSQL

```powershell
docker-compose up -d postgres jaeger
```

### 2. Update appsettings.Development.json

Ensure the configuration points to localhost:

```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Database=ecommerce;Username=ecommerce;Password=ecommerce;Port=5432"
    },
    "OpenTelemetry": {
        "ServiceName": "ECommerce.Backend.Dev",
        "ServiceVersion": "0.1.17",
        "EnableConsoleExporter": true,
        "OtlpEndpoint": "http://localhost:4317"
    }
}
```

### 3. Run the application

```powershell
dotnet run
```

## Exploring Traces

### Example: Order Creation Flow

1. Create an order via API:

```powershell
POST http://localhost/api/v1/orders
```

2. In Jaeger UI, you'll see:
    - HTTP request span
    - Database query spans
    - Custom business logic spans (if instrumented)
    - Dependencies between services

### Trace Details Include:

-   **Duration**: How long each operation took
-   **Tags**: Custom metadata (order ID, customer ID, etc.)
-   **Events**: Significant moments in the trace
-   **Errors**: Exception details if any occurred
-   **Database Queries**: SQL statements and execution time

## Console Exporter (Development)

For development without Jaeger, enable console exporter:

```json
{
    "OpenTelemetry": {
        "EnableConsoleExporter": true,
        "OtlpEndpoint": ""
    }
}
```

Traces will be printed to the console output.

## Production Configuration

For production, configure your OTLP endpoint to point to your observability backend:

### Grafana Cloud

```json
{
    "OpenTelemetry": {
        "ServiceName": "ECommerce.Backend",
        "ServiceVersion": "0.1.17",
        "EnableConsoleExporter": false,
        "OtlpEndpoint": "https://otlp-gateway-prod-us-central-0.grafana.net/otlp"
    }
}
```

### Honeycomb

```json
{
    "OpenTelemetry": {
        "ServiceName": "ECommerce.Backend",
        "ServiceVersion": "0.1.17",
        "EnableConsoleExporter": false,
        "OtlpEndpoint": "https://api.honeycomb.io:443"
    }
}
```

### Self-hosted Jaeger

```json
{
    "OpenTelemetry": {
        "ServiceName": "ECommerce.Backend",
        "ServiceVersion": "0.1.17",
        "EnableConsoleExporter": false,
        "OtlpEndpoint": "http://jaeger-collector:4317"
    }
}
```

## Environment Variables

You can also configure OpenTelemetry via environment variables:

```powershell
$env:OpenTelemetry__ServiceName="ECommerce.Backend"
$env:OpenTelemetry__ServiceVersion="0.1.17"
$env:OpenTelemetry__EnableConsoleExporter="true"
$env:OpenTelemetry__OtlpEndpoint="http://localhost:4317"

dotnet run
```

## Troubleshooting

### No traces appearing in Jaeger

1. Check if Jaeger is running:

    ```powershell
    docker ps | Select-String jaeger
    ```

2. Check Jaeger logs:

    ```powershell
    docker logs ecommerce-jaeger
    ```

3. Verify OTLP endpoint is correct in configuration

4. Enable console exporter to see traces locally

### Application can't connect to database

1. Check if PostgreSQL is running:

    ```powershell
    docker ps | Select-String postgres
    ```

2. Verify connection string in configuration

3. Check PostgreSQL logs:
    ```powershell
    docker logs ecommerce-postgres
    ```

### Port conflicts

If ports are already in use, modify the ports in docker-compose.yml:

```yaml
ports:
    - "5433:5432" # Change PostgreSQL port to 5433
    - "8081:8080" # Change API port to 8081
```

## Jaeger UI Features

### Search Traces

-   Filter by service, operation, tags
-   Set time range
-   Set min/max duration

### Trace Timeline

-   Visualize span relationships
-   Identify bottlenecks
-   See parallel vs sequential operations

### Service Dependencies

-   View service architecture
-   Identify service dependencies
-   Monitor inter-service communication

### Metrics

-   Request rate
-   Error rate
-   Duration percentiles (p50, p75, p95, p99)

## Next Steps

1. Review [OPENTELEMETRY_GUIDE.md](OPENTELEMETRY_GUIDE.md) for custom instrumentation
2. Add custom spans to your business logic
3. Configure alerts based on trace data
4. Set up distributed tracing across microservices

## Resources

-   [Jaeger Documentation](https://www.jaegertracing.io/docs/)
-   [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
-   [Docker Compose Documentation](https://docs.docker.com/compose/)

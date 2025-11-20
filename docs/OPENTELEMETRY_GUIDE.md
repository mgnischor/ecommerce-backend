# OpenTelemetry Integration Guide

This document describes the OpenTelemetry implementation in the E-Commerce Backend API, including configuration, usage examples, and best practices.

## Overview

OpenTelemetry provides comprehensive observability for the application through:

-   **Distributed Tracing**: Track requests across service boundaries
-   **Metrics**: Monitor application performance and health
-   **Instrumentation**: Automatic instrumentation for ASP.NET Core, HTTP clients, and Entity Framework Core

## Configuration

### appsettings.json

```json
{
    "OpenTelemetry": {
        "ServiceName": "ECommerce.Backend",
        "ServiceVersion": "0.1.17",
        "EnableConsoleExporter": false,
        "OtlpEndpoint": ""
    }
}
```

### appsettings.Development.json

```json
{
    "OpenTelemetry": {
        "ServiceName": "ECommerce.Backend.Dev",
        "ServiceVersion": "0.1.17",
        "EnableConsoleExporter": true,
        "OtlpEndpoint": "http://localhost:4317"
    }
}
```

### Configuration Options

-   **ServiceName**: The name of the service (used in telemetry data)
-   **ServiceVersion**: The version of the service
-   **EnableConsoleExporter**: Enable console output for traces and metrics (useful for development)
-   **OtlpEndpoint**: OTLP (OpenTelemetry Protocol) endpoint for exporting data (e.g., Jaeger, Grafana Tempo)

## Automatic Instrumentation

The following components are automatically instrumented:

### 1. ASP.NET Core

-   HTTP request/response tracking
-   Request duration and status codes
-   User agent and content length
-   Exception tracking

### 2. HTTP Client

-   Outgoing HTTP request tracking
-   Response status codes
-   Request URIs

### 3. Entity Framework Core

-   Database query tracking
-   SQL statement text
-   Query duration
-   Command timeout

### 4. .NET Runtime

-   Garbage collection metrics
-   Thread pool metrics
-   Exception counters

## Custom Tracing

### Using ActivityHelper

```csharp
using ECommerce.API.Extensions;

public class OrderService
{
    public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
    {
        // Start a custom span
        using var activity = ActivityHelper.StartActivity("CreateOrder");

        // Add custom tags
        activity?.SetTag("customer.id", dto.CustomerId);
        activity?.SetTag("order.items.count", dto.Items.Count);
        activity?.SetTag("order.total", dto.TotalAmount);

        try
        {
            // Your business logic here
            var order = await ProcessOrderAsync(dto);

            // Add success event
            ActivityHelper.AddEvent("OrderCreated", new ActivityTagsCollection
            {
                { "order.id", order.Id },
                { "order.number", order.OrderNumber }
            });

            ActivityHelper.SetStatus(ActivityStatusCode.Ok);
            return order;
        }
        catch (Exception ex)
        {
            // Record exception
            ActivityHelper.RecordException(ex);
            throw;
        }
    }
}
```

### Direct Activity API

```csharp
using System.Diagnostics;
using ECommerce.API.Extensions;

public class PaymentService
{
    public async Task ProcessPaymentAsync(Guid orderId, decimal amount)
    {
        using var activity = OpenTelemetryExtensions.ActivitySource.StartActivity(
            "ProcessPayment",
            ActivityKind.Client
        );

        activity?.SetTag("order.id", orderId);
        activity?.SetTag("payment.amount", amount);
        activity?.SetTag("payment.currency", "USD");

        // Your payment processing logic
        await ChargeCustomerAsync(orderId, amount);

        activity?.SetStatus(ActivityStatusCode.Ok);
    }
}
```

## Adding Tags in Controllers

```csharp
using ECommerce.API.Extensions;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        // Add custom tags to the current HTTP span
        ActivityHelper.AddTag("product.id", id);
        ActivityHelper.AddTag("user.role", User.FindFirst("role")?.Value);

        var product = await _productRepository.GetByIdAsync(id);

        if (product == null)
        {
            ActivityHelper.AddEvent("ProductNotFound");
            return NotFound();
        }

        return Ok(product);
    }
}
```

## Recording Events

Events are useful for recording significant moments in a trace:

```csharp
public async Task ProcessInventoryAsync(Guid productId, int quantity)
{
    using var activity = ActivityHelper.StartActivity("ProcessInventory");

    // Record inventory check event
    ActivityHelper.AddEvent("InventoryChecked", new ActivityTagsCollection
    {
        { "product.id", productId },
        { "quantity.requested", quantity }
    });

    var available = await CheckAvailabilityAsync(productId);

    if (available < quantity)
    {
        // Record insufficient inventory event
        ActivityHelper.AddEvent("InsufficientInventory", new ActivityTagsCollection
        {
            { "quantity.available", available },
            { "quantity.needed", quantity }
        });

        throw new InsufficientInventoryException();
    }

    // Record reservation event
    ActivityHelper.AddEvent("InventoryReserved", new ActivityTagsCollection
    {
        { "quantity.reserved", quantity }
    });
}
```

## Error Tracking

```csharp
public async Task<Result> ProcessOrderAsync(Guid orderId)
{
    using var activity = ActivityHelper.StartActivity("ProcessOrder");
    activity?.SetTag("order.id", orderId);

    try
    {
        await ValidateOrderAsync(orderId);
        await ChargePaymentAsync(orderId);
        await UpdateInventoryAsync(orderId);

        ActivityHelper.SetStatus(ActivityStatusCode.Ok);
        return Result.Success();
    }
    catch (ValidationException ex)
    {
        ActivityHelper.RecordException(ex);
        ActivityHelper.SetStatus(ActivityStatusCode.Error, "Order validation failed");
        return Result.Failure(ex.Message);
    }
    catch (PaymentException ex)
    {
        ActivityHelper.RecordException(ex);
        ActivityHelper.SetStatus(ActivityStatusCode.Error, "Payment processing failed");
        return Result.Failure(ex.Message);
    }
    catch (Exception ex)
    {
        ActivityHelper.RecordException(ex);
        ActivityHelper.SetStatus(ActivityStatusCode.Error, "Unexpected error");
        throw;
    }
}
```

## Exporting Telemetry Data

### Console Exporter (Development)

Set `EnableConsoleExporter: true` in appsettings.Development.json to see traces and metrics in the console output.

### OTLP Exporter (Production)

Configure the `OtlpEndpoint` to send data to your observability backend:

#### Jaeger

```json
{
    "OpenTelemetry": {
        "OtlpEndpoint": "http://jaeger:4317"
    }
}
```

#### Grafana Tempo

```json
{
    "OpenTelemetry": {
        "OtlpEndpoint": "http://tempo:4317"
    }
}
```

#### Honeycomb

```json
{
    "OpenTelemetry": {
        "OtlpEndpoint": "https://api.honeycomb.io:443"
    }
}
```

## Running with Docker Compose

Example docker-compose.yml with Jaeger:

```yaml
version: "3.8"

services:
    ecommerce-backend:
        build: .
        environment:
            - OpenTelemetry__OtlpEndpoint=http://jaeger:4317
        depends_on:
            - jaeger
            - postgres

    jaeger:
        image: jaegertracing/all-in-one:latest
        ports:
            - "16686:16686" # Jaeger UI
            - "4317:4317" # OTLP gRPC receiver
            - "4318:4318" # OTLP HTTP receiver
        environment:
            - COLLECTOR_OTLP_ENABLED=true

    postgres:
        image: postgres:16
        environment:
            - POSTGRES_USER=ecommerce
            - POSTGRES_PASSWORD=ecommerce
            - POSTGRES_DB=ecommerce
        ports:
            - "5432:5432"
```

Access Jaeger UI at: http://localhost:16686

## Best Practices

### 1. Meaningful Span Names

Use descriptive, hierarchical names:

-   ✅ `ProcessOrder`
-   ✅ `Payment.Charge`
-   ✅ `Inventory.Reserve`
-   ❌ `Execute`
-   ❌ `DoWork`

### 2. Add Context with Tags

```csharp
activity?.SetTag("customer.id", customerId);
activity?.SetTag("order.total", total);
activity?.SetTag("payment.method", "credit_card");
```

### 3. Record Significant Events

```csharp
ActivityHelper.AddEvent("PaymentAuthorized");
ActivityHelper.AddEvent("InventoryReserved");
ActivityHelper.AddEvent("EmailSent");
```

### 4. Use Appropriate Activity Kinds

-   `ActivityKind.Internal` - Internal operations (default)
-   `ActivityKind.Client` - Outgoing requests
-   `ActivityKind.Server` - Incoming requests (handled automatically by ASP.NET Core)
-   `ActivityKind.Producer` - Message queue producers
-   `ActivityKind.Consumer` - Message queue consumers

### 5. Always Use 'using' Statement

```csharp
using var activity = ActivityHelper.StartActivity("OperationName");
// Activity is automatically disposed and duration is recorded
```

### 6. Don't Over-Instrument

-   Instrument at logical boundaries (service methods, external calls)
-   Avoid instrumenting every method
-   Focus on business-critical operations

## Monitoring Metrics

The following metrics are automatically collected:

### ASP.NET Core Metrics

-   `http.server.request.duration` - HTTP request duration
-   `http.server.active_requests` - Active HTTP requests

### HTTP Client Metrics

-   `http.client.request.duration` - HTTP client request duration
-   `http.client.active_requests` - Active HTTP client requests

### .NET Runtime Metrics

-   `process.runtime.dotnet.gc.collections.count` - GC collection count
-   `process.runtime.dotnet.gc.heap.size` - GC heap size
-   `process.runtime.dotnet.thread_pool.threads.count` - Thread pool thread count
-   `process.runtime.dotnet.exceptions.count` - Exception count

## Troubleshooting

### No traces appearing

1. Check if OpenTelemetry is enabled in configuration
2. Verify `OtlpEndpoint` is correct
3. Enable `EnableConsoleExporter` to see traces in console
4. Check if the collector/backend is running

### Traces not connected

1. Ensure W3C trace context is propagated in HTTP headers
2. Check if all services use compatible OpenTelemetry versions
3. Verify trace context is passed through async boundaries

### High overhead

1. Reduce sampling rate (configure sampler in OpenTelemetryExtensions)
2. Disable detailed SQL statement collection if not needed
3. Review custom instrumentation for excessive span creation

## Resources

-   [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/instrumentation/net/)
-   [OpenTelemetry Semantic Conventions](https://opentelemetry.io/docs/specs/semconv/)
-   [Jaeger Documentation](https://www.jaegertracing.io/docs/)
-   [Grafana Tempo Documentation](https://grafana.com/docs/tempo/)

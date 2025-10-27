using System.Diagnostics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ECommerce.API.Extensions;

/// <summary>
/// Extension methods for configuring OpenTelemetry observability in the application.
/// Provides distributed tracing, metrics, and integration with various instrumentation libraries.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Application activity source for custom tracing spans.
    /// Use this to create custom spans throughout the application.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new("ECommerce.Backend", "1.0.0");

    /// <summary>
    /// Configures OpenTelemetry with tracing and metrics for the application.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <returns>The web application builder for chaining.</returns>
    public static WebApplicationBuilder AddOpenTelemetryObservability(
        this WebApplicationBuilder builder
    )
    {
        var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "ECommerce.Backend";
        var serviceVersion = builder.Configuration["OpenTelemetry:ServiceVersion"] ?? "1.0.0";
        var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
        var enableConsoleExporter = builder.Configuration.GetValue<bool>(
            "OpenTelemetry:EnableConsoleExporter"
        );

        // Configure OpenTelemetry with Tracing and Metrics
        builder
            .Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource
                    .AddService(
                        serviceName: serviceName,
                        serviceVersion: serviceVersion,
                        serviceInstanceId: Environment.MachineName
                    )
                    .AddAttributes(
                        new Dictionary<string, object>
                        {
                            ["deployment.environment"] = builder.Environment.EnvironmentName,
                            ["host.name"] = Environment.MachineName,
                        }
                    );
            })
            .WithTracing(tracing =>
            {
                tracing
                    // Add ASP.NET Core instrumentation for incoming HTTP requests
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, httpRequest) =>
                        {
                            activity.SetTag(
                                "http.request.user_agent",
                                httpRequest.Headers.UserAgent.ToString()
                            );
                            activity.SetTag(
                                "http.request.content_length",
                                httpRequest.ContentLength
                            );
                        };
                        options.EnrichWithHttpResponse = (activity, httpResponse) =>
                        {
                            activity.SetTag(
                                "http.response.content_length",
                                httpResponse.ContentLength
                            );
                        };
                    })
                    // Add HTTP Client instrumentation for outgoing HTTP requests
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = (activity, httpRequest) =>
                        {
                            activity.SetTag(
                                "http.client.request.uri",
                                httpRequest.RequestUri?.ToString()
                            );
                        };
                        options.EnrichWithHttpResponseMessage = (activity, httpResponse) =>
                        {
                            activity.SetTag(
                                "http.client.response.status_code",
                                (int)httpResponse.StatusCode
                            );
                        };
                    })
                    // Add Entity Framework Core instrumentation
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.EnrichWithIDbCommand = (activity, command) =>
                        {
                            activity.SetTag("db.command.timeout", command.CommandTimeout);
                            activity.SetTag("db.command.text", command.CommandText);
                        };
                    })
                    // Add custom application activity sources
                    .AddSource(ActivitySource.Name)
                    .AddSource("ECommerce.Application.Accounting");

                // Add console exporter for development
                if (enableConsoleExporter)
                {
                    tracing.AddConsoleExporter();
                }

                // Add OTLP exporter if endpoint is configured
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    // Add ASP.NET Core metrics
                    .AddAspNetCoreInstrumentation()
                    // Add HTTP Client metrics
                    .AddHttpClientInstrumentation()
                    // Add .NET Runtime metrics (GC, thread pool, etc.)
                    .AddRuntimeInstrumentation();

                // Add console exporter for development
                if (enableConsoleExporter)
                {
                    metrics.AddConsoleExporter();
                }

                // Add OTLP exporter if endpoint is configured
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
                }
            });

        return builder;
    }
}

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace ShopHub.Api.Extensions;

public static class ObservabilityExtensions
{
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "shophub";
        var otlpEndpoint = builder.Configuration["OpenTelemetry:Endpoint"];

        builder.Host.UseSerilog((ctx, services, config) =>
        {
            config
                .ReadFrom.Configuration(ctx.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("service.name", serviceName)
                .Enrich.WithProperty("deployment.environment", ctx.HostingEnvironment.EnvironmentName);

            if (!string.IsNullOrEmpty(otlpEndpoint))
                config.WriteTo.OpenTelemetry(o =>
                {
                    o.Endpoint = otlpEndpoint;
                    o.Protocol = OtlpProtocol.Grpc;
                    o.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = serviceName,
                        ["deployment.environment"] = ctx.HostingEnvironment.EnvironmentName
                    };
                });
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(serviceName)
                .AddAttributes([new("deployment.environment", builder.Environment.EnvironmentName)]))
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation(o => o.RecordException = true)
                 .AddHttpClientInstrumentation();

                if (!string.IsNullOrEmpty(otlpEndpoint))
                    t.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
            })
            .WithMetrics(m =>
            {
                m.AddAspNetCoreInstrumentation()
                 .AddHttpClientInstrumentation()
                 .AddRuntimeInstrumentation();

                if (!string.IsNullOrEmpty(otlpEndpoint))
                    m.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
            });

        return builder;
    }
}

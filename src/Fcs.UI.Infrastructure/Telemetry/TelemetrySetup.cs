using OpenTelemetry;
using OpenTelemetry.Trace;
using Serilog;

namespace Fcs.UI.Infrastructure.Telemetry;

public static class TelemetrySetup
{
    public static void ConfigureOpenTelemetry()
    {
        Sdk.CreateTracerProviderBuilder()
            .AddSource("Fcs.UI")
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri("http://otel-collector:4318");
            })
            .Build();
    }

    public static void ConfigureSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.Seq("http://seq:5341")
            .Enrich.WithProperty("App", "fcs-maui-app")
            .CreateLogger();
    }
}

using Akka.Hosting;
using Akka.IOListener;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Instrumentation.Sockets;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using LogLevel = Akka.Event.LogLevel;

var hostBuilder = new HostBuilder();

hostBuilder.ConfigureServices((context, services) =>
{
    services.AddLogging(builder =>
    {
        builder.ClearProviders();
        builder.AddConsole();
        
        var resourceBuilder = ResourceBuilder.CreateDefault();
        resourceBuilder
            .AddEnvironmentVariableDetector()
            .AddTelemetrySdk();

        builder.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resourceBuilder);
        }); 
    });
    
    services.AddAkka("MyActorSystem", (builder, sp) =>
    {
        builder
            .ConfigureLoggers(akkaLoggers =>
            {
                // use MSFT.EXT.Logging for OTEL export
                akkaLoggers.ClearLoggers().AddLoggerFactory();
            })
            .WithActors((system, registry, resolver) =>
            {
                var helloActor = system.ActorOf(Props.Create(() => new HelloActor()), "hello-actor");
                registry.Register<HelloActor>(helloActor);
            })
            .WithActors((system, registry, resolver) =>
            {
                var timerActorProps =
                    resolver.Props<TimerActor>(); // uses Msft.Ext.DI to inject reference to helloActor
                var timerActor = system.ActorOf(timerActorProps, "timer-actor");
                registry.Register<TimerActor>(timerActor);
            });
    });

    services.AddOpenTelemetry()
        .ConfigureResource(builder =>
        {
            builder
                .AddEnvironmentVariableDetector()
                .AddTelemetrySdk();
        })
        .WithMetrics(c =>
        {
            c.AddRuntimeInstrumentation()
                .AddSocketInstrumentation()
                .AddConsoleExporter();
        });
});

var host = hostBuilder.Build();

await host.RunAsync();
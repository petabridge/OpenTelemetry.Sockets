using System;
using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;

namespace OpenTelemetry.Instrumentation.Sockets;

public static class SocketInstrumentationExtensions
{
    public const string MeterName = "OpenTelemetry.Sockets";
    public static readonly Meter SocketMeter = new Meter(MeterName);

    public static MeterProviderBuilder AddSocketInstrumentation(this MeterProviderBuilder builder,
        Action<ISocketTelemetryConfigurator> configure)
    {
        var configurator = new DefaultSocketTelemetryConfigurator(builder);
        configure(configurator);
        return builder;
    }

    public static MeterProviderBuilder AddSocketInstrumentation(this MeterProviderBuilder builder)
    {
        return AddSocketInstrumentation(builder, configurator =>
        {
            configurator.AddTcpConnectionInstrumentation().AddTcpStatisticsInstrumentation();
        });
    }
}
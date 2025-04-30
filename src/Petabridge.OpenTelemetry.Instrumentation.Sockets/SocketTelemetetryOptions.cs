using System;
using OpenTelemetry.Metrics;

namespace OpenTelemetry.Instrumentation.Sockets;

/// <summary>
/// Configures the OpenTelemetry instrumentation for sockets.
/// </summary>
public interface ISocketTelemetryConfigurator
{
    /// <summary>
    /// How often to collect telemetry data from the socket instrumentation on instruments
    /// that support it.
    /// </summary>
    public TimeSpan CollectionInterval { get; set; }
}

/// <summary>
/// INTERNAL API - private implementation details for configuring OpenTelemetry instrumentation for sockets.
/// </summary>
internal sealed class DefaultSocketTelemetryConfigurator : ISocketTelemetryConfigurator
{
    public static readonly TimeSpan DefaultCollectionInterval = TimeSpan.FromSeconds(10);
    
    public DefaultSocketTelemetryConfigurator(MeterProviderBuilder builder)
    {
        builder.AddMeter(SocketInstrumentationExtensions.MeterName);
    }

    public TimeSpan CollectionInterval { get; set; } = DefaultCollectionInterval;
}
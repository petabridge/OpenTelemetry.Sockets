using OpenTelemetry.Metrics;

namespace OpenTelemetry.Instrumentation.Sockets;

/// <summary>
/// Configures the OpenTelemetry instrumentation for sockets.
/// </summary>
public interface ISocketTelemetryConfigurator
{
    
}

/// <summary>
/// INTERNAL API - private implementation details for configuring OpenTelemetry instrumentation for sockets.
/// </summary>
internal sealed class DefaultSocketTelemetryConfigurator : ISocketTelemetryConfigurator
{
    public DefaultSocketTelemetryConfigurator(MeterProviderBuilder builder)
    {
        builder.AddMeter(SocketInstrumentationExtensions.MeterName);
    }
}
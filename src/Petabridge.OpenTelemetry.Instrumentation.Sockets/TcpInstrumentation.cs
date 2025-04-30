using System.Net;
using System.Net.NetworkInformation;
using OpenTelemetry.Metrics;
using static OpenTelemetry.Instrumentation.Sockets.TcpInstrumentationMeter;

namespace OpenTelemetry.Instrumentation.Sockets;

/// <summary>
/// Publicly available instrumentation code for tracking TCP connections and listeners
/// </summary>
public static class TcpInstrumentation
{
    /// <summary>
    /// Subscribe to TCP connection and listener events and track them using OpenTelemetry metrics.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="onlyCareAboutTheseEndpoints">Optional. When populated, we will only report
    /// about activity involving these endpoints.</param>
    /// <remarks>
    /// This method will automatically start tracking TCP connection and listener activity in the background - captures
    /// everything happening in the current environment.
    /// </remarks>
    public static MeterProviderBuilder AddTcpConnectionInstrumentation(this MeterProviderBuilder builder,
        IPEndPoint[]? onlyCareAboutTheseEndpoints = null)
    {
        builder.AddMeter(MeterName);

        // these will just run in the background
        var (connectionsTracker, tcpListenersTracker) = TrackTcpActivity(KeepConnectionData, KeepListenerData);

        return builder;

        bool KeepListenerData(IPEndPoint endpoint)
        {
            if (onlyCareAboutTheseEndpoints is null || onlyCareAboutTheseEndpoints.Length == 0) return true;

            foreach (var caredAboutEndpoint in onlyCareAboutTheseEndpoints)
            {
                if (caredAboutEndpoint.Equals(endpoint)) return true;
            }

            return false;
        }

        bool KeepConnectionData(TcpConnectionInformation connection)
        {
            if (onlyCareAboutTheseEndpoints is null || onlyCareAboutTheseEndpoints.Length == 0) return true;

            foreach (var endpoint in onlyCareAboutTheseEndpoints)
            {
                if (connection.LocalEndPoint.Equals(endpoint) || connection.RemoteEndPoint.Equals(endpoint)) return true;
            }

            return false;
        }
    }
}
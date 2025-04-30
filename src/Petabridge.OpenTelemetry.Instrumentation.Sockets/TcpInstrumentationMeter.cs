using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace OpenTelemetry.Instrumentation.Sockets;

/// <summary>
/// INTERNAL API - private implementation details for tracking TCP connections and listeners
/// </summary>
internal static class TcpInstrumentationMeter
{
    public const string MeterName = "OpenTelemetry.TcpInstrumentation";
    public static readonly Meter Meter = new Meter(MeterName);
    public const string ActiveTcpConnectionsName = "tcp.connections.active";
    public const string TcpConnectionsUnit = "tcp_connections";
    public const string ActiveTcpListenersName = "tcp.listeners.active";
    public const string TcpListenersUnit = "tcp_listeners";

    private static readonly Predicate<TcpConnectionInformation> DefaultTcpConnectionKeepFn = information => true;
    private static readonly Predicate<IPEndPoint> DefaultListeningEndpointKeepFn = ip => true;

    public static (ObservableGauge<int> connectionsTracker, ObservableGauge<int> tcpListenersTracker) TrackTcpActivity(
        Predicate<TcpConnectionInformation>? keepConnectionData = null,
        Predicate<IPEndPoint>? keepListenerData = null)
    {
        var connectionsTracker = TrackActiveTcpConnections(ActiveTcpConnectionsName, "Active TCP Connections", keepData: keepConnectionData);
        var tcpListenersTracker = TrackActiveTcpListeners(ActiveTcpListenersName, "Active TCP Listeners", keepData:keepListenerData);
        return (connectionsTracker, tcpListenersTracker);
    }

    public static ObservableGauge<int> TrackActiveTcpConnections(string metricName, string description,
        string units = TcpConnectionsUnit, Predicate<TcpConnectionInformation>? keepData = null)
        => Meter.CreateObservableGauge<int>(metricName,
            () =>
            {
                keepData ??= DefaultTcpConnectionKeepFn;
                var rawMeasurements = new Dictionary<TcpState, int>();
                var realMeasurements = Array.Empty<Measurement<int>>();
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();

                // for each connection, if the connection is to be kept, add it to the measurements
                foreach (var connection in connections)
                    if (keepData(connection))
                    {
                        rawMeasurements.TryAdd(connection.State, 0);
                        rawMeasurements[connection.State]++;
                    }

                // bail out early if we have no data
                if (rawMeasurements.Count == 0)
                    return realMeasurements;

                realMeasurements = new Measurement<int>[rawMeasurements.Count];
                var i = 0;
                foreach (var (state, count) in rawMeasurements)
                {
                    // some boxing here, but it's fine
                    var tag = new KeyValuePair<string, object?>("tcp.state", state);
                    realMeasurements[i] = new Measurement<int>(count, tag);
                    i++;
                }

                return realMeasurements;
            }, units, description);

    public static ObservableGauge<int> TrackActiveTcpListeners(string metricName, string description,
        string units = TcpListenersUnit, Predicate<IPEndPoint>? keepData = null)
        => Meter.CreateObservableGauge<int>(metricName,
            () =>
            {
                keepData ??= DefaultListeningEndpointKeepFn;
                
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                var endPoints = properties.GetActiveTcpListeners().Where(c => keepData(c));
                return endPoints.Select(c =>
                {
                    var tags = new[]
                    {
                        new KeyValuePair<string, object?>("listener_addr", c.Address),
                        new KeyValuePair<string, object?>("listener_port", c.Port),
                        new KeyValuePair<string, object?>("ip_family", c.AddressFamily)
                    };
                    return new Measurement<int>(1, tags);
                });
            }, units, description);
}
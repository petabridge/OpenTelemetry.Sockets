// -----------------------------------------------------------------------
// <copyright file="TcpInstrumentationMeter.cs" company="Petabridge, LLC">
//      Copyright (C) 2025 - 2025 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static OpenTelemetry.Instrumentation.Sockets.SocketInstrumentationExtensions;

namespace OpenTelemetry.Instrumentation.Sockets;

/// <summary>
///     INTERNAL API - private implementation details for tracking TCP connections and listeners
/// </summary>
internal static class TcpInstrumentationMeter
{
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
        var connectionsTracker = TrackActiveTcpConnections(ActiveTcpConnectionsName, "Active TCP Connections",
            keepData: keepConnectionData);
        var tcpListenersTracker =
            TrackActiveTcpListeners(ActiveTcpListenersName, "Active TCP Listeners", keepData: keepListenerData);
        return (connectionsTracker, tcpListenersTracker);
    }
    
    public static ObservableGauge<long> TrackTcpIpStatistics(IpFamily family)
    {
        return family switch
        {
            IpFamily.IPv6 => TrackTcpIpv6Statistics(),
            IpFamily.IPv4 => TrackTcpIpv4Statistics(),
            _ => throw new ArgumentOutOfRangeException(nameof(family), family, $"Invalid IP family: {family}")
        };
    }

    private static ObservableGauge<long> TrackTcpIpv4Statistics()
    {
        return SocketMeter.CreateObservableGauge("tcp.stats.ipv4",
            () =>
            {
                var stats = IPGlobalProperties.GetIPGlobalProperties().GetTcpIPv4Statistics();
                return RecordTcpStats(AddressFamily.InterNetwork, stats);
            }, "observed", "TCP IPv4 statistics");
    }

    private static IEnumerable<Measurement<long>> RecordTcpStats(AddressFamily family, 
        TcpStatistics statistics)
    {
        var measures = new List<Measurement<long>>();
        SetMetric("connections_accepted", statistics.ConnectionsAccepted);
        SetMetric("connections_initiated", statistics.ConnectionsInitiated);
        SetMetric("connections_reset", statistics.ResetConnections);
        SetMetric("connections_established", statistics.CurrentConnections);
        SetMetric("connections_cumulative", statistics.CumulativeConnections);
        SetMetric("maximum_connections", statistics.MaximumConnections);
        SetMetric("errors_received", statistics.ErrorsReceived);
        SetMetric("segments_received", statistics.SegmentsReceived);
        SetMetric("segments_sent", statistics.SegmentsSent);
        SetMetric("segments_resent", statistics.SegmentsResent);
        SetMetric("failed_connection_attempts", statistics.FailedConnectionAttempts);
        SetMetric("resets_sent", statistics.ResetsSent);
        return measures;

        void SetMetric(string name, long value)
        {
            var tagList = new TagList { { "metric", name } };
            measures.Add(new Measurement<long>(value, tagList));
        }
    }

    private static ObservableGauge<long> TrackTcpIpv6Statistics()
    {
        return SocketMeter.CreateObservableGauge("tcp.stats.ipv6",
            () =>
            {
                var stats = IPGlobalProperties.GetIPGlobalProperties().GetTcpIPv6Statistics();
                return RecordTcpStats(AddressFamily.InterNetworkV6, stats);
            }, "observed", "TCP IPv6 statistics");
    }

    public static ObservableGauge<int> TrackActiveTcpConnections(string metricName, string description,
        string units = TcpConnectionsUnit, Predicate<TcpConnectionInformation>? keepData = null)
    {
        return SocketMeter.CreateObservableGauge(metricName,
            () =>
            {
                keepData ??= DefaultTcpConnectionKeepFn;
                var rawMeasurements = new Dictionary<TcpState, int>();
                var realMeasurements = Array.Empty<Measurement<int>>();
                var properties = IPGlobalProperties.GetIPGlobalProperties();
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
    }

    public static ObservableGauge<int> TrackActiveTcpListeners(string metricName, string description,
        string units = TcpListenersUnit, Predicate<IPEndPoint>? keepData = null)
    {
        return SocketMeter.CreateObservableGauge(metricName,
            () =>
            {
                keepData ??= DefaultListeningEndpointKeepFn;

                /*
                 * These are all local listener addresses - the cardinality is low,
                 * so it shouldn't be a risk to add them all to the tags from a cost
                 * perspective.
                 */
                var properties = IPGlobalProperties.GetIPGlobalProperties();
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
}
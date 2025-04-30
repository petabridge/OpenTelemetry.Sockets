using System.Net.Sockets;

namespace OpenTelemetry.Instrumentation.Sockets;

/// <summary>
/// The set of IP families we can track.
/// </summary>
/// <remarks>
/// We don't use <see cref="AddressFamily"/> because it includes lots of unsupported address types.
/// </remarks>
public enum IpFamily
{
    IPv4,
    IPv6
}
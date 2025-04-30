// -----------------------------------------------------------------------
// <copyright file="IpFamily.cs" company="Petabridge, LLC">
//      Copyright (C) 2025 - 2025 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System.Net.Sockets;

namespace OpenTelemetry.Instrumentation.Sockets;

/// <summary>
///     The set of IP families we can track.
/// </summary>
/// <remarks>
///     We don't use <see cref="AddressFamily" /> because it includes lots of unsupported address types.
/// </remarks>
public enum IpFamily
{
    IPv4,
    IPv6
}
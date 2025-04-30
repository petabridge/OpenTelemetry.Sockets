// -----------------------------------------------------------------------
// <copyright file="SocketTelemetetryOptions.cs" company="Petabridge, LLC">
//      Copyright (C) 2025 - 2025 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using OpenTelemetry.Metrics;

namespace OpenTelemetry.Instrumentation.Sockets;

/// <summary>
///     Configures the OpenTelemetry instrumentation for sockets.
/// </summary>
public interface ISocketTelemetryConfigurator
{
}

/// <summary>
///     INTERNAL API - private implementation details for configuring OpenTelemetry instrumentation for sockets.
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
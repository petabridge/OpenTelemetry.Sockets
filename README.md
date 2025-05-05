# Petabridge.OpenTelemetry.Instrumentation.Sockets

[![Nuget version](https://img.shields.io/nuget/v/Petabridge.OpenTelemetry.Instrumentation.Sockets)](https://www.nuget.org/packages/Petabridge.OpenTelemetry.Instrumentation.Sockets/) [![Nuget downloads](https://img.shields.io/nuget/dt/Petabridge.OpenTelemetry.Instrumentation.Sockets)](https://www.nuget.org/packages/Petabridge.OpenTelemetry.Instrumentation.Sockets/)

An [OpenTelemetry](https://opentelemetry.io/) instrumentation package for collecting detailed metrics about TCP socket
activity on Windows and Linux systems. This was developed by [Petabridge](https://petabridge.com/) to support our
development work on [Akka.NET](https://getakka.net/).

## What It Does

This package provides instrumentation for monitoring TCP connections and statistics directly from the operating system.
It allows you to gather metrics such as:

* Number of active TCP connections
* Number of established TCP connections
* Number of TCP connections in specific states (e.g., `TIME_WAIT`, `CLOSE_WAIT`)
* TCP errors and resets
* Segments sent and received

This information is crucial for understanding network performance, diagnosing connectivity issues, and monitoring the
health of network-intensive applications.

**Supported Platforms:**

* Windows (all stats)
* Linux (active TCP connections and listeners only - see limited by .NET runtime platform support)

## :warning: Important Caveat: System-Wide Data Collection

Please be aware that this instrumentation gathers TCP data **for the entire operating system environment**, not just for
the specific process where the instrumentation is running. The underlying APIs (`iphlpapi.dll` on Windows,
`/proc/net/tcp*` on Linux) provide a system-wide view of TCP activity.

If you need process-specific network metrics, you will need to use different instrumentation methods or correlate this
data with process-specific identifiers if possible.

## Installation

You can install this package via the .NET CLI:

```powershell
dotnet add package Petabridge.OpenTelemetry.Instrumentation.Sockets
```

Or via the NuGet Package Manager console:

```powershell
Install-Package Petabridge.OpenTelemetry.Instrumentation.Sockets
```

## Usage

To enable TCP socket instrumentation, add it to your OpenTelemetry `MeterProvider` configuration.

**Basic Configuration:**

This example shows how to add the instrumentation with default settings:

```csharp
using OpenTelemetry.Metrics;
using Petabridge.OpenTelemetry.Instrumentation.Sockets; // <-- Add this using statement

// ... inside your service configuration (e.g., Program.cs or Startup.cs)
services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder
            // Add other meters as needed
            .AddConsoleExporter() // Example exporter
            .AddSocketInstrumentation(); // <-- Add this line
    });
```

**Custom Configuration:**

You can customize the instrumentation behavior, such as the collection interval and which specific metrics to enable.

```csharp
using System;
using OpenTelemetry.Metrics;
using Petabridge.OpenTelemetry.Instrumentation.Sockets; // <-- Add this using statement

// ... inside your service configuration
services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder
            // Add other meters as needed
            .AddConsoleExporter() // Example exporter
            .AddSocketInstrumentation(configurator => // <-- Configure options
            {
                // Explicitly enable TCP connection state metrics (enabled by default)
                configurator.AddTcpConnectionInstrumentation();

                // Explicitly enable TCP statistics metrics (enabled by default)
                configurator.AddTcpStatisticsInstrumentation();
            });
    });
```

See the [OpenTelemetry .NET documentation](https://opentelemetry.io/docs/instrumentation/net/getting-started/) for more
details on configuring exporters and the SDK.

Copyright 2015-2025 [Petabridge](https://petabridge.com/), LLC.

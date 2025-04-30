// -----------------------------------------------------------------------
// <copyright file="CrashTest.cs" company="Petabridge, LLC">
//      Copyright (C) 2025 - 2025 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Instrumentation.Sockets;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Petabridge.OpenTelemetry.Instrumentation.Sockets.Tests;

public class CrashTest
{
    [Fact]
    public async Task ShouldCrashWithIllegalInterval()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            // arrange
            var hostBuilder = new HostBuilder();
            hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddOpenTelemetry()
                    .ConfigureResource(builder =>
                    {
                        builder
                            .AddEnvironmentVariableDetector()
                            .AddTelemetrySdk();
                    })
                    .WithMetrics(c =>
                    {
                        c
                            .AddSocketInstrumentation(configurator =>
                            {
                                configurator.CollectionInterval = TimeSpan.Zero;
                                configurator.AddTcpConnectionInstrumentation();
                                configurator.AddTcpStatisticsInstrumentation();
                            })
                            .AddConsoleExporter();
                    });
            });

            // act
            using var builder = hostBuilder.Build();
            await builder.StartAsync();

            // assert
            await Task.Delay(TimeSpan.FromSeconds(5)); // long enough for metrics collection
            await builder.StopAsync();
        });
    }

    [Fact]
    public async Task ShouldLaunchSocketMetricsWithoutCrash()
    {
        // arrange
        var hostBuilder = new HostBuilder();
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddOpenTelemetry()
                .ConfigureResource(builder =>
                {
                    builder
                        .AddEnvironmentVariableDetector()
                        .AddTelemetrySdk();
                })
                .WithMetrics(c =>
                {
                    c
                        .AddSocketInstrumentation(configurator =>
                        {
                            configurator.CollectionInterval = TimeSpan.FromSeconds(1);
                            configurator.AddTcpConnectionInstrumentation();
                            configurator.AddTcpStatisticsInstrumentation();
                        })
                        .AddConsoleExporter();
                });
        });

        // act
        using var builder = hostBuilder.Build();
        await builder.StartAsync();

        // assert
        await Task.Delay(TimeSpan.FromSeconds(5)); // long enough for metrics collection
        await builder.StopAsync();
    }
}
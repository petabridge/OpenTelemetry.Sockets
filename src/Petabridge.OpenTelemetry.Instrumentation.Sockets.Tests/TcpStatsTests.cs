// -----------------------------------------------------------------------
// <copyright file="TcpStatsTests.cs" company="Petabridge, LLC">
//      Copyright (C) 2025 - 2025 Petabridge, LLC <https://petabridge.com>
// </copyright>
// -----------------------------------------------------------------------

using System.Net.NetworkInformation;
using OpenTelemetry.Instrumentation.Sockets;
using static OpenTelemetry.Instrumentation.Sockets.TcpInstrumentationMeter;

namespace Petabridge.OpenTelemetry.Instrumentation.Sockets.Tests;

public class TcpStatsTests
{
    private static TcpStatistics GetStats(IpFamily ipFamily)
    {
        return ipFamily switch
        {
            IpFamily.IPv4 => IPGlobalProperties.GetIPGlobalProperties().GetTcpIPv4Statistics(),
            IpFamily.IPv6 => IPGlobalProperties.GetIPGlobalProperties().GetTcpIPv6Statistics(),
            _ => throw new ArgumentOutOfRangeException(nameof(ipFamily), ipFamily, null)
        };
    }
    
    [Theory]
    [InlineData(IpFamily.IPv6)]
    [InlineData(IpFamily.IPv4)]
    public void TcpStats_should_return_correct_values(IpFamily ipFamily)
    {
        // arrange
        var tcpStats = RecordTcpStats(GetStats(ipFamily)).ToList();
        
        // assert
        Assert.NotNull(tcpStats);
        Assert.NotEmpty(tcpStats);
        
        // at least one stat should have a non-zero value
        var hasNonZeroValue = tcpStats.Any(stat => stat.Value > 0);
        Assert.True(hasNonZeroValue, "At least one TCP stat should have a non-zero value.");
    }
}
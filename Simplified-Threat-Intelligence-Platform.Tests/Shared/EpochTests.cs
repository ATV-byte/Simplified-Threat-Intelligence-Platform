using System;
using Simplified_Threat_Intelligence_Platform.Shared;
using Xunit;

namespace Simplified_Threat_Intelligence_Platform.Tests.Shared
{
    public class EpochTests
    {
        [Fact]
        public void Now_Returns_Current_Seconds()
        {
            long before = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long now = Epoch.Now();
            long after = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Assert.InRange(now, before, after);
        }

        [Fact]
        public void DaysAgo_Computes_Correct_Offset()
        {
            const int days = 5;
            long now = Epoch.Now();
            long expected = now - days * 24L * 3600L;
            long actual = Epoch.DaysAgo(days);
            Assert.InRange(actual, expected - 1, expected + 1);
        }
    }
}

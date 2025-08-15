using System;
using Xunit;
using Simplified_Threat_Intelligence_Platform.Shared;

namespace Simplified_Threat_Intelligence_Platform.Tests.Shared
{
    public class EpochTests
    {
        private const long OneDaySeconds = 24L * 3600L;
        private const int ToleranceSeconds = 2; // pentru drift de execuție

        [Fact]
        public void Now_ShouldReturnCurrentUnixTimeSeconds_CloseToSystemNow()
        {
            // Arrange
            long before = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Act
            long actual = Epoch.Now();

            long after = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Assert: actual trebuie să fie între before-±tol și after+±tol
            Assert.InRange(actual, before - ToleranceSeconds, after + ToleranceSeconds);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(30)]
        [InlineData(-3)] // zile negative -> viitor
        public void DaysAgo_ShouldReturn_NowMinusDaysWithinTolerance(int days)
        {
            // Arrange
            long baselineNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long expected = baselineNow - days * OneDaySeconds;

            // Act
            long actual = Epoch.DaysAgo(days);

            // Assert
            long diff = Math.Abs(actual - expected);
            Assert.True(diff <= ToleranceSeconds,
                $"Expected ~{expected} but got {actual}. Diff={diff}s, tol={ToleranceSeconds}s");
        }
    }
}

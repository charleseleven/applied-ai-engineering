using AgilePredict.Models.Configuration;
using AgilePredict.Models.DTOs.SprintRisk;
using AgilePredict.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace AgilePredict.Tests.Services.SprintRisk
{
    public class SprintCapacityRiskCalculatorTests
    {
        private static readonly DateTime Now = new(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        private static SprintCapacityRiskCalculator CreateCalculator(double criticalShortfallRatio = 0.5)
        {
            var config = new SprintRiskBotConfiguration { CriticalShortfallRatio = criticalShortfallRatio };
            return new SprintCapacityRiskCalculator(Options.Create(config));
        }

        private static ActiveSprintSnapshot Sprint(int total, int completed, DateTime start, DateTime end) => new()
        {
            SprintId = 1,
            SprintTitle = "Sprint X",
            TotalStoryPoints = total,
            CompletedStoryPoints = completed,
            StartDate = start,
            EndDate = end
        };

        [Fact]
        public void Assess_WhenHistoricalCapacityCoversRemainingWork_IsOnTrack()
        {
            // 10 dias restantes, capacidade de 2 pts/dia => 20 pts projetados, faltam 15 pts.
            var sprint = Sprint(total: 25, completed: 10, start: Now.AddDays(-4), end: Now.AddDays(10));

            var result = CreateCalculator().Assess(sprint, historicalDailyCapacity: 2.0, asOfUtc: Now);

            Assert.False(result.IsAtRisk);
            Assert.Equal(SprintRiskLevel.OnTrack, result.RiskLevel);
            Assert.Equal(0, result.ProjectedShortfall);
            Assert.Equal(15, result.RemainingStoryPoints);
        }

        [Fact]
        public void Assess_WhenHistoricalCapacityFallsShortBySmallMargin_IsWarning()
        {
            // 10 dias restantes, capacidade de 1 pt/dia => 10 pts projetados, faltam 15 pts
            // (déficit de 5 pts = 33% dos 15 restantes, abaixo do limiar crítico de 50%).
            var sprint = Sprint(total: 25, completed: 10, start: Now.AddDays(-4), end: Now.AddDays(10));

            var result = CreateCalculator().Assess(sprint, historicalDailyCapacity: 1.0, asOfUtc: Now);

            Assert.True(result.IsAtRisk);
            Assert.Equal(SprintRiskLevel.Warning, result.RiskLevel);
            Assert.Equal(5, result.ProjectedShortfall);
        }

        [Fact]
        public void Assess_WhenHistoricalCapacityFallsShortByLargeMargin_IsCritical()
        {
            // 10 dias restantes, capacidade de 0 pts/dia => 0 pts projetados, faltam 15 pts
            // (déficit de 15 pts = 100% dos 15 restantes, acima do limiar crítico de 50%).
            var sprint = Sprint(total: 25, completed: 10, start: Now.AddDays(-4), end: Now.AddDays(10));

            var result = CreateCalculator().Assess(sprint, historicalDailyCapacity: 0.0, asOfUtc: Now);

            Assert.True(result.IsAtRisk);
            Assert.Equal(SprintRiskLevel.Critical, result.RiskLevel);
            Assert.Equal(15, result.ProjectedShortfall);
        }

        [Fact]
        public void Assess_WhenSprintAlreadyEndedWithPendingPoints_IsAtRiskWithUndefinedRequiredPace()
        {
            var sprint = Sprint(total: 10, completed: 4, start: Now.AddDays(-14), end: Now.AddDays(-1));

            var result = CreateCalculator().Assess(sprint, historicalDailyCapacity: 2.0, asOfUtc: Now);

            Assert.True(result.IsAtRisk);
            Assert.Equal(0, result.RemainingDays);
            Assert.Equal(6, result.RemainingStoryPoints);
            Assert.Null(result.RequiredDailyPace);
        }

        [Fact]
        public void Assess_WhenSprintAlreadyCompleted_IsOnTrackWithNoRequiredPace()
        {
            var sprint = Sprint(total: 10, completed: 10, start: Now.AddDays(-14), end: Now.AddDays(-1));

            var result = CreateCalculator().Assess(sprint, historicalDailyCapacity: 2.0, asOfUtc: Now);

            Assert.False(result.IsAtRisk);
            Assert.Equal(0, result.RemainingStoryPoints);
            Assert.Equal(0, result.RequiredDailyPace);
        }

        [Fact]
        public void AssessAll_MapsEachSprintIndependently()
        {
            var sprints = new List<ActiveSprintSnapshot>
            {
                Sprint(total: 10, completed: 10, start: Now.AddDays(-5), end: Now.AddDays(5)),
                Sprint(total: 10, completed: 0, start: Now.AddDays(-5), end: Now.AddDays(1))
            };

            var results = CreateCalculator().AssessAll(sprints, historicalDailyCapacity: 1.0, asOfUtc: Now);

            Assert.Equal(2, results.Count);
            Assert.False(results[0].IsAtRisk);
            Assert.True(results[1].IsAtRisk);
        }
    }
}

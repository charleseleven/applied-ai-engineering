using AgilePredict.Models.Configuration;
using AgilePredict.Models.DTOs.SprintRisk;
using AgilePredict.Services;
using AgilePredict.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AgilePredict.Tests.Services.SprintRisk
{
    public class SprintRiskBotOrchestratorTests
    {
        private readonly Mock<ISprintRiskDataSource> _dataSourceMock = new();
        private readonly Mock<ISprintCapacityRiskCalculator> _calculatorMock = new();
        private readonly Mock<ISprintRiskBotInsightGenerator> _insightGeneratorMock = new();
        private readonly Mock<ILogger<SprintRiskBotOrchestrator>> _loggerMock = new();

        private SprintRiskBotOrchestrator CreateOrchestrator(SprintRiskBotConfiguration? config = null)
        {
            config ??= new SprintRiskBotConfiguration
            {
                HistoricalSprintsLookbackCount = 5,
                DefaultDailyCapacityPoints = 3.0,
                CompletedSprintStatus = "Completed"
            };

            return new SprintRiskBotOrchestrator(
                _dataSourceMock.Object,
                _calculatorMock.Object,
                _insightGeneratorMock.Object,
                Options.Create(config),
                _loggerMock.Object);
        }

        private List<ActiveSprintSnapshot> SetupActiveSprints()
        {
            var activeSprints = new List<ActiveSprintSnapshot> { new() { SprintId = 1, SprintTitle = "Sprint 1" } };
            _dataSourceMock.Setup(d => d.GetActiveSprintsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(activeSprints);
            return activeSprints;
        }

        [Fact]
        public async Task GenerateReportAsync_WithHistoricalSamples_UsesAverageAsCapacity()
        {
            var activeSprints = SetupActiveSprints();
            var historicalSamples = new List<SprintHistoricalVelocitySample>
            {
                new() { SprintId = 10, SprintTitle = "Sprint -2", DailyVelocity = 2.0 },
                new() { SprintId = 11, SprintTitle = "Sprint -1", DailyVelocity = 4.0 }
            };
            _dataSourceMock
                .Setup(d => d.GetHistoricalVelocitySamplesAsync(5, "Completed", It.IsAny<CancellationToken>()))
                .ReturnsAsync(historicalSamples);

            var assessments = new List<SprintRiskAssessment> { new() { SprintId = 1 } };
            _calculatorMock
                .Setup(c => c.AssessAll(activeSprints, 3.0, null))
                .Returns(assessments);

            _insightGeneratorMock
                .Setup(g => g.GenerateAsync(assessments, 3.0, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SprintRiskInsightResult { Success = true, Narrative = "ok", Recommendations = new List<string> { "faz isso" } });

            var orchestrator = CreateOrchestrator();
            var report = await orchestrator.GenerateReportAsync();

            Assert.Equal(3.0, report.HistoricalDailyCapacity);
            Assert.Equal(2, report.HistoricalSprintsUsed);
            Assert.False(report.AiInsightUnavailable);
            Assert.Equal("ok", report.AiNarrative);
            Assert.Single(report.AiRecommendations);
        }

        [Fact]
        public async Task GenerateReportAsync_WithoutHistoricalSamples_FallsBackToDefaultCapacity()
        {
            var activeSprints = SetupActiveSprints();
            _dataSourceMock
                .Setup(d => d.GetHistoricalVelocitySamplesAsync(5, "Completed", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SprintHistoricalVelocitySample>());

            var assessments = new List<SprintRiskAssessment> { new() { SprintId = 1 } };
            _calculatorMock
                .Setup(c => c.AssessAll(activeSprints, 3.0, null))
                .Returns(assessments);

            _insightGeneratorMock
                .Setup(g => g.GenerateAsync(assessments, 3.0, 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SprintRiskInsightResult { Success = true, Narrative = "ok" });

            var orchestrator = CreateOrchestrator();
            var report = await orchestrator.GenerateReportAsync();

            Assert.Equal(3.0, report.HistoricalDailyCapacity);
            Assert.Equal(0, report.HistoricalSprintsUsed);
        }

        [Fact]
        public async Task GenerateReportAsync_WhenInsightGeneratorFails_MarksUnavailableButKeepsAssessments()
        {
            var activeSprints = SetupActiveSprints();
            _dataSourceMock
                .Setup(d => d.GetHistoricalVelocitySamplesAsync(5, "Completed", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SprintHistoricalVelocitySample>());

            var assessments = new List<SprintRiskAssessment> { new() { SprintId = 1, SprintTitle = "Sprint 1" } };
            _calculatorMock
                .Setup(c => c.AssessAll(activeSprints, 3.0, null))
                .Returns(assessments);

            _insightGeneratorMock
                .Setup(g => g.GenerateAsync(assessments, 3.0, 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SprintRiskInsightResult { Success = false, ErrorMessage = "falhou" });

            var orchestrator = CreateOrchestrator();
            var report = await orchestrator.GenerateReportAsync();

            Assert.True(report.AiInsightUnavailable);
            Assert.Single(report.Assessments);
            Assert.Equal("Sprint 1", report.Assessments[0].SprintTitle);
        }

        [Fact]
        public async Task GenerateReportAsync_WhenInsightGeneratorThrows_MarksUnavailableWithoutPropagating()
        {
            SetupActiveSprints();
            _dataSourceMock
                .Setup(d => d.GetHistoricalVelocitySamplesAsync(5, "Completed", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SprintHistoricalVelocitySample>());

            _calculatorMock
                .Setup(c => c.AssessAll(It.IsAny<IReadOnlyList<ActiveSprintSnapshot>>(), 3.0, null))
                .Returns(new List<SprintRiskAssessment>());

            _insightGeneratorMock
                .Setup(g => g.GenerateAsync(It.IsAny<IReadOnlyList<SprintRiskAssessment>>(), 3.0, 0, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var orchestrator = CreateOrchestrator();
            var report = await orchestrator.GenerateReportAsync();

            Assert.True(report.AiInsightUnavailable);
        }
    }
}

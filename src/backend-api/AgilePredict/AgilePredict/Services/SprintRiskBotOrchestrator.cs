using AgilePredict.Models.Configuration;
using AgilePredict.Models.DTOs.SprintRisk;
using AgilePredict.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AgilePredict.Services
{
    /// <summary>
    /// Implementação do pipeline do bot preditivo de risco de Sprint (PBI #160/#163, Feature #147).
    /// </summary>
    public class SprintRiskBotOrchestrator : ISprintRiskBotOrchestrator
    {
        private readonly ISprintRiskDataSource _dataSource;
        private readonly ISprintCapacityRiskCalculator _calculator;
        private readonly ISprintRiskBotInsightGenerator _insightGenerator;
        private readonly SprintRiskBotConfiguration _configuration;
        private readonly ILogger<SprintRiskBotOrchestrator> _logger;

        public SprintRiskBotOrchestrator(
            ISprintRiskDataSource dataSource,
            ISprintCapacityRiskCalculator calculator,
            ISprintRiskBotInsightGenerator insightGenerator,
            IOptions<SprintRiskBotConfiguration> configuration,
            ILogger<SprintRiskBotOrchestrator> logger)
        {
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            _insightGenerator = insightGenerator ?? throw new ArgumentNullException(nameof(insightGenerator));
            _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SprintRiskReport> GenerateReportAsync(CancellationToken cancellationToken = default)
        {
            var activeSprints = await _dataSource.GetActiveSprintsAsync(cancellationToken);
            var historicalSamples = await _dataSource.GetHistoricalVelocitySamplesAsync(
                _configuration.HistoricalSprintsLookbackCount, _configuration.CompletedSprintStatus, cancellationToken);

            var historicalDailyCapacity = historicalSamples.Count > 0
                ? historicalSamples.Average(s => s.DailyVelocity)
                : _configuration.DefaultDailyCapacityPoints;

            var assessments = _calculator.AssessAll(activeSprints, historicalDailyCapacity);

            var report = new SprintRiskReport
            {
                HistoricalDailyCapacity = Math.Round(historicalDailyCapacity, 2),
                HistoricalSprintsUsed = historicalSamples.Count,
                HistoricalVelocitySeries = historicalSamples.ToList(),
                Assessments = assessments.ToList()
            };

            // Uma falha na LLM não deve derrubar o relatório estatístico, que já é útil por si só.
            try
            {
                var insight = await _insightGenerator.GenerateAsync(
                    assessments, historicalDailyCapacity, historicalSamples.Count, cancellationToken);

                if (insight.Success)
                {
                    report.AiNarrative = insight.Narrative;
                    report.AiRecommendations = insight.Recommendations;
                }
                else
                {
                    report.AiInsightUnavailable = true;
                    _logger.LogWarning("Insight do bot de risco indisponível: {Error}", insight.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                report.AiInsightUnavailable = true;
                _logger.LogError(ex, "Erro inesperado ao gerar insight do bot de risco de Sprint");
            }

            return report;
        }
    }
}

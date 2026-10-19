using AgilePredict.Models.Configuration;
using AgilePredict.Models.DTOs.SprintRisk;
using AgilePredict.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AgilePredict.Services
{
    /// <summary>
    /// Implementação da Task #161: calculador puro e determinístico, testável isoladamente sem
    /// banco de dados ou LLM.
    /// </summary>
    public class SprintCapacityRiskCalculator : ISprintCapacityRiskCalculator
    {
        private readonly SprintRiskBotConfiguration _configuration;

        public SprintCapacityRiskCalculator(IOptions<SprintRiskBotConfiguration> configuration)
        {
            _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IReadOnlyList<SprintRiskAssessment> AssessAll(
            IReadOnlyList<ActiveSprintSnapshot> sprints, double historicalDailyCapacity, DateTime? asOfUtc = null)
        {
            ArgumentNullException.ThrowIfNull(sprints);
            return sprints.Select(s => Assess(s, historicalDailyCapacity, asOfUtc)).ToList();
        }

        public SprintRiskAssessment Assess(ActiveSprintSnapshot sprint, double historicalDailyCapacity, DateTime? asOfUtc = null)
        {
            ArgumentNullException.ThrowIfNull(sprint);

            var now = asOfUtc ?? DateTime.UtcNow;
            var elapsedDays = Math.Max(0, (now - sprint.StartDate).TotalDays);
            var remainingDays = Math.Max(0, (sprint.EndDate - now).TotalDays);
            var remainingStoryPoints = Math.Max(0, sprint.TotalStoryPoints - sprint.CompletedStoryPoints);

            // AC da PBI #160: cruza o total de Story Points entregues com a capacidade diária
            // restante do time (capacidade histórica × dias restantes) para projetar se a
            // Sprint fecha no prazo com o ritmo atual do time.
            var projectedAdditionalPoints = historicalDailyCapacity * remainingDays;
            var projectedTotalCompletion = sprint.CompletedStoryPoints + projectedAdditionalPoints;
            var projectedShortfall = Math.Max(0, sprint.TotalStoryPoints - projectedTotalCompletion);

            // Nulo (em vez de double.PositiveInfinity) quando a Sprint já terminou com pontos
            // pendentes: não há mais dias para distribuir o ritmo, e Infinity não é serializável
            // em JSON (System.Text.Json lança exceção ao tentar serializar Infinity/NaN).
            double? requiredDailyPace = remainingDays > 0
                ? remainingStoryPoints / remainingDays
                : (remainingStoryPoints > 0 ? null : 0);

            var isAtRisk = projectedShortfall > 0;

            var riskLevel = !isAtRisk
                ? SprintRiskLevel.OnTrack
                : projectedShortfall >= remainingStoryPoints * _configuration.CriticalShortfallRatio
                    ? SprintRiskLevel.Critical
                    : SprintRiskLevel.Warning;

            return new SprintRiskAssessment
            {
                SprintId = sprint.SprintId,
                SprintTitle = sprint.SprintTitle,
                TotalStoryPoints = sprint.TotalStoryPoints,
                CompletedStoryPoints = sprint.CompletedStoryPoints,
                RemainingStoryPoints = remainingStoryPoints,
                ElapsedDays = Math.Round(elapsedDays, 1),
                RemainingDays = Math.Round(remainingDays, 1),
                RequiredDailyPace = requiredDailyPace.HasValue ? Math.Round(requiredDailyPace.Value, 2) : null,
                ProjectedAdditionalPoints = Math.Round(projectedAdditionalPoints, 2),
                ProjectedTotalCompletion = Math.Round(projectedTotalCompletion, 2),
                ProjectedShortfall = Math.Round(projectedShortfall, 2),
                IsAtRisk = isAtRisk,
                RiskLevel = riskLevel
            };
        }
    }
}

using AgilePredict.Models.DTOs.SprintRisk;

namespace AgilePredict.Services.Interfaces
{
    /// <summary>
    /// Regra de negócio analítica pura (Task #161): nenhuma dependência de EF Core/IO, recebe
    /// dados já materializados e cruza os Story Points entregues com a capacidade diária
    /// restante do time (projeção = capacidade histórica diária × dias restantes da Sprint).
    /// </summary>
    public interface ISprintCapacityRiskCalculator
    {
        SprintRiskAssessment Assess(ActiveSprintSnapshot sprint, double historicalDailyCapacity, DateTime? asOfUtc = null);

        IReadOnlyList<SprintRiskAssessment> AssessAll(
            IReadOnlyList<ActiveSprintSnapshot> sprints, double historicalDailyCapacity, DateTime? asOfUtc = null);
    }
}

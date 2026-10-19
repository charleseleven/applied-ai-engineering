using AgilePredict.Models.DTOs.SprintRisk;

namespace AgilePredict.Services.Interfaces
{
    /// <summary>
    /// Implementação da Task #162: monta um System Prompt dinâmico (interpolado com os dados
    /// calculados em tempo real pela API — capacidade histórica do time, janela de observação)
    /// e envia o payload de risco para a LLM configurada, retornando o relatório textual do bot.
    /// </summary>
    public interface ISprintRiskBotInsightGenerator
    {
        Task<SprintRiskInsightResult> GenerateAsync(
            IReadOnlyList<SprintRiskAssessment> assessments,
            double historicalDailyCapacity,
            int historicalSprintsUsed,
            CancellationToken cancellationToken = default);
    }
}

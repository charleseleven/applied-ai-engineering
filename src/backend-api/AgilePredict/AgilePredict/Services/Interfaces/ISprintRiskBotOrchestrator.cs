using AgilePredict.Models.DTOs.SprintRisk;

namespace AgilePredict.Services.Interfaces
{
    /// <summary>
    /// Orquestra o pipeline completo do bot preditivo de risco (PBI #160): coleta -> capacidade
    /// histórica -> avaliação de risco por Sprint -> insight de IA -> relatório (Task #165 consome via API).
    /// </summary>
    public interface ISprintRiskBotOrchestrator
    {
        Task<SprintRiskReport> GenerateReportAsync(CancellationToken cancellationToken = default);
    }
}

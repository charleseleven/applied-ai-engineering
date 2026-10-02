using AgilePredict.Models.DTOs;

namespace AgilePredict.Services.Interfaces
{
    /// <summary>
    /// Analisa as Sprints ativas e identifica gargalos (ritmo de conclusão atrasado em
    /// relação ao tempo restante), gravando um SprintHealthAlert quando necessário.
    /// </summary>
    public interface ISprintHealthAnalyzer
    {
        /// <summary>
        /// Avalia todas as Sprints ativas e retorna quantos novos alertas foram gerados
        /// </summary>
        Task<int> AnalyzeActiveSprintsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retorna o resumo de saúde (completion% vs tempo decorrido%) de cada Sprint ativa,
        /// sem gravar alertas — usado pelo painel preditivo.
        /// </summary>
        Task<IReadOnlyList<SprintHealthSummary>> GetHealthSummaryAsync(CancellationToken cancellationToken = default);
    }
}

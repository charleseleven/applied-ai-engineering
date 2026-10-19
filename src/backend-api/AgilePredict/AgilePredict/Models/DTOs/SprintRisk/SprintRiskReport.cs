namespace AgilePredict.Models.DTOs.SprintRisk
{
    /// <summary>
    /// Relatório formatado do bot preditivo de risco de Sprint (PBI #160/#163): reúne a
    /// capacidade histórica do time, a avaliação de risco de cada Sprint ativa e o diagnóstico
    /// textual gerado por IA. É o contrato consumido pelo dashboard executivo (Task #164/#165).
    /// </summary>
    public class SprintRiskReport
    {
        public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Capacidade diária do time (Story Points/dia) usada na projeção, já com a média das
        /// Sprints históricas aplicada (ou o valor padrão, quando não há histórico suficiente).
        /// </summary>
        public double HistoricalDailyCapacity { get; set; }

        /// <summary>
        /// Quantas Sprints concluídas efetivamente entraram na média acima (0 quando o valor
        /// padrão de configuração foi usado por falta de histórico).
        /// </summary>
        public int HistoricalSprintsUsed { get; set; }

        public List<SprintHistoricalVelocitySample> HistoricalVelocitySeries { get; set; } = new();

        public List<SprintRiskAssessment> Assessments { get; set; } = new();

        /// <summary>
        /// Diagnóstico textual gerado pelo bot (LLM) a partir das avaliações de risco.
        /// </summary>
        public string AiNarrative { get; set; } = string.Empty;

        public List<string> AiRecommendations { get; set; } = new();

        /// <summary>
        /// Indica se a geração do insight de IA falhou; as avaliações estatísticas permanecem
        /// válidas (não dependem da LLM), mas o frontend deve sinalizar a falha.
        /// </summary>
        public bool AiInsightUnavailable { get; set; }
    }
}

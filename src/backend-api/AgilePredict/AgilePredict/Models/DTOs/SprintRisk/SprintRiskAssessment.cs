namespace AgilePredict.Models.DTOs.SprintRisk
{
    public enum SprintRiskLevel
    {
        OnTrack,
        Warning,
        Critical
    }

    /// <summary>
    /// Resultado da análise de risco de uma Sprint ativa (Task #161, AC da PBI #160): cruza o
    /// total de Story Points já entregues com a capacidade diária histórica do time projetada
    /// para os dias restantes da Sprint.
    /// </summary>
    public class SprintRiskAssessment
    {
        public int SprintId { get; set; }

        public string SprintTitle { get; set; } = string.Empty;

        public int TotalStoryPoints { get; set; }

        public int CompletedStoryPoints { get; set; }

        public int RemainingStoryPoints { get; set; }

        public double ElapsedDays { get; set; }

        public double RemainingDays { get; set; }

        /// <summary>
        /// Ritmo diário (Story Points/dia) que o time precisaria manter pelo resto da Sprint
        /// para concluir todos os pontos restantes no prazo. Nulo quando a Sprint já terminou
        /// com pontos pendentes (não há mais dias para distribuir o ritmo).
        /// </summary>
        public double? RequiredDailyPace { get; set; }

        /// <summary>
        /// Pontos adicionais que o time deve entregar até o fim da Sprint, projetados a partir
        /// da capacidade histórica diária (capacidade × dias restantes).
        /// </summary>
        public double ProjectedAdditionalPoints { get; set; }

        /// <summary>
        /// Total projetado de Story Points concluídos até o fim da Sprint (entregues + projeção).
        /// </summary>
        public double ProjectedTotalCompletion { get; set; }

        /// <summary>
        /// Déficit projetado: quantos Story Points ficariam sem concluir, mantido o ritmo
        /// histórico do time. Zero quando a Sprint está projetada para fechar no prazo.
        /// </summary>
        public double ProjectedShortfall { get; set; }

        public bool IsAtRisk { get; set; }

        public SprintRiskLevel RiskLevel { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace AgilePredict.Models.Configuration
{
    /// <summary>
    /// Configurações do agente autônomo de monitoramento de saúde das Sprints
    /// </summary>
    public class SprintHealthMonitorConfiguration
    {
        public const string SectionName = "SprintHealthMonitor";

        /// <summary>
        /// Intervalo entre execuções do agente (em horas)
        /// </summary>
        [Range(1, 168, ErrorMessage = "O intervalo deve estar entre 1 e 168 horas")]
        public int CheckIntervalHours { get; set; } = 24;

        /// <summary>
        /// Diferença mínima (em pontos percentuais) entre tempo decorrido da Sprint e
        /// percentual de Story Points concluídos para considerar a Sprint em risco
        /// </summary>
        [Range(1, 100, ErrorMessage = "O threshold deve estar entre 1 e 100")]
        public int RiskThresholdPercentPoints { get; set; } = 20;
    }
}

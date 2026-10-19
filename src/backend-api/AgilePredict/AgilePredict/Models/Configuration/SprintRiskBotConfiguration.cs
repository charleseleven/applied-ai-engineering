using System.ComponentModel.DataAnnotations;

namespace AgilePredict.Models.Configuration
{
    /// <summary>
    /// Configurações do bot preditivo de risco de Sprint (PBI #160/#163, Feature #147).
    /// </summary>
    public class SprintRiskBotConfiguration
    {
        /// <summary>
        /// Seção no appsettings.json
        /// </summary>
        public const string SectionName = "SprintRiskBot";

        /// <summary>
        /// Quantas Sprints concluídas mais recentes entram na média de capacidade diária
        /// histórica do time (Story Points/dia).
        /// </summary>
        [Range(1, 50, ErrorMessage = "O número de sprints históricas deve estar entre 1 e 50")]
        public int HistoricalSprintsLookbackCount { get; set; } = 5;

        /// <summary>
        /// Capacidade diária padrão (Story Points/dia), usada quando não há Sprints concluídas
        /// suficientes para calcular uma capacidade histórica real.
        /// </summary>
        [Range(0.1, 1000, ErrorMessage = "A capacidade diária padrão deve ser maior que zero")]
        public double DefaultDailyCapacityPoints { get; set; } = 3.0;

        /// <summary>
        /// Valor exato de <c>Sprint.Status</c> usado para identificar Sprints já concluídas
        /// (fonte da capacidade histórica).
        /// </summary>
        public string CompletedSprintStatus { get; set; } = "Completed";

        /// <summary>
        /// Fração do déficit projetado em relação aos Story Points restantes a partir da qual o
        /// risco é classificado como "Critical" em vez de "Warning" (ex.: 0.5 = déficit projetado
        /// de 50% ou mais dos pontos restantes).
        /// </summary>
        [Range(0.01, 1.0, ErrorMessage = "A fração crítica deve estar entre 0.01 e 1.0")]
        public double CriticalShortfallRatio { get; set; } = 0.5;
    }
}

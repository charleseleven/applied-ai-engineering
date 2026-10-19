namespace AgilePredict.Models.DTOs.SprintRisk
{
    /// <summary>
    /// Retrato de uma Sprint ativa já materializado a partir do banco relacional via EF Core
    /// (Task #161), desacoplado de qualquer entidade/navegação do EF — é o contrato de entrada
    /// do calculador puro de risco (<see cref="AgilePredict.Services.Interfaces.ISprintCapacityRiskCalculator"/>).
    /// </summary>
    public class ActiveSprintSnapshot
    {
        public int SprintId { get; set; }

        public string SprintTitle { get; set; } = string.Empty;

        public int TotalStoryPoints { get; set; }

        public int CompletedStoryPoints { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}

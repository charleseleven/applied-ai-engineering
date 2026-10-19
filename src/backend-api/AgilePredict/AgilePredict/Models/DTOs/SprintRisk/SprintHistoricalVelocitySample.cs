namespace AgilePredict.Models.DTOs.SprintRisk
{
    /// <summary>
    /// Velocidade diária (Story Points/dia) apurada de uma Sprint já concluída, usada para
    /// compor a capacidade histórica do time.
    /// </summary>
    public class SprintHistoricalVelocitySample
    {
        public int SprintId { get; set; }

        public string SprintTitle { get; set; } = string.Empty;

        public double DailyVelocity { get; set; }
    }
}

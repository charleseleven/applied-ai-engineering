namespace AgilePredict.Models.DTOs
{
    /// <summary>
    /// Resumo de saúde de uma Sprint ativa, usado pelo painel preditivo (PBI #230)
    /// </summary>
    public class SprintHealthSummary
    {
        public int SprintId { get; set; }
        public string SprintTitle { get; set; } = string.Empty;
        public double CompletionPercent { get; set; }
        public double TimeElapsedPercent { get; set; }
        public bool IsAtRisk { get; set; }
    }
}

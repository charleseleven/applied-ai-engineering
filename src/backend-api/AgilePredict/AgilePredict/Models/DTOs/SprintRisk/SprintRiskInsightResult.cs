namespace AgilePredict.Models.DTOs.SprintRisk
{
    /// <summary>
    /// Resultado da geração de insight do bot de risco via LLM (Task #162): diagnóstico textual
    /// e sugestões de mitigação, a partir das avaliações de risco calculadas.
    /// </summary>
    public class SprintRiskInsightResult
    {
        public bool Success { get; set; }

        public string Narrative { get; set; } = string.Empty;

        public List<string> Recommendations { get; set; } = new();

        public string? ErrorMessage { get; set; }
    }
}

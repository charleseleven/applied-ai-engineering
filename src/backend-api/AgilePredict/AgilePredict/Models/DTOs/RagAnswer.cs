namespace AgilePredict.Models.DTOs
{
    /// <summary>
    /// Resultado do pipeline RAG: a resposta da IA e as fontes usadas como contexto
    /// </summary>
    public class RagAnswer
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<RagSource> Sources { get; set; } = [];
    }

    /// <summary>
    /// Uma das Tasks recuperadas por similaridade e usadas como contexto na resposta
    /// </summary>
    public class RagSource
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public double Similarity { get; set; }
    }
}

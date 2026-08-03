namespace AgilePredict.Models.DTOs
{
    /// <summary>
    /// Response retornada pela API LLM
    /// </summary>
    public class LlmResponse
    {
        /// <summary>
        /// Indica se a requisição foi bem-sucedida
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Texto da resposta gerada pela IA
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Mensagem de erro, se houver
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Modelo utilizado pela LLM
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// Número de tokens utilizados na requisição
        /// </summary>
        public int? TokensUsed { get; set; }

        /// <summary>
        /// Timestamp da resposta
        /// </summary>
        public DateTime ResponseTime { get; set; } = DateTime.UtcNow;
    }
}

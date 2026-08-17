using System.ComponentModel.DataAnnotations;

namespace AgilePredict.Models.Configuration
{
    /// <summary>
    /// Configurações para integração com API de embeddings (Gemini)
    /// </summary>
    public class EmbeddingConfiguration
    {
        /// <summary>
        /// Seção no appsettings.json
        /// </summary>
        public const string SectionName = "GeminiSettings";

        /// <summary>
        /// URL base da API de embeddings
        /// </summary>
        [Required(ErrorMessage = "A URL da API é obrigatória")]
        [Url(ErrorMessage = "A URL deve ser válida")]
        public string ApiUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/";

        /// <summary>
        /// Chave de API para autenticação (NÃO deve ser hardcoded)
        /// Deve vir do Secret Manager (dev) ou Environment Variables (prod)
        /// </summary>
        [Required(ErrorMessage = "A API Key é obrigatória")]
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Modelo de embedding a ser utilizado
        /// </summary>
        public string Model { get; set; } = "gemini-embedding-2";

        /// <summary>
        /// Dimensionalidade do vetor de saída (128 a 3072; 768 é um bom equilíbrio custo/qualidade)
        /// </summary>
        [Range(128, 3072, ErrorMessage = "OutputDimensionality deve estar entre 128 e 3072")]
        public int OutputDimensionality { get; set; } = 768;

        /// <summary>
        /// Timeout para requisições HTTP (em segundos)
        /// </summary>
        [Range(5, 300, ErrorMessage = "O timeout deve estar entre 5 e 300 segundos")]
        public int TimeoutSeconds { get; set; } = 30;
    }
}

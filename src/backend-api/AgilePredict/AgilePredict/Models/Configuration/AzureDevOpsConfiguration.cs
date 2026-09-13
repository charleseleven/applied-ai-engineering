using System.ComponentModel.DataAnnotations;

namespace AgilePredict.Models.Configuration
{
    /// <summary>
    /// Configurações para integração com a API REST do Azure DevOps (coleta de Work Items e histórico de status).
    /// </summary>
    public class AzureDevOpsConfiguration
    {
        /// <summary>
        /// Seção no appsettings.json
        /// </summary>
        public const string SectionName = "AzureDevOpsSettings";

        /// <summary>
        /// URL base da API do Azure DevOps.
        /// </summary>
        [Required(ErrorMessage = "A URL da API é obrigatória")]
        [Url(ErrorMessage = "A URL deve ser válida")]
        public string ApiUrl { get; set; } = "https://dev.azure.com/";

        /// <summary>
        /// Nome da organização no Azure DevOps.
        /// </summary>
        [Required(ErrorMessage = "A organização é obrigatória")]
        public string Organization { get; set; } = string.Empty;

        /// <summary>
        /// Nome do projeto no Azure DevOps.
        /// </summary>
        [Required(ErrorMessage = "O projeto é obrigatório")]
        public string Project { get; set; } = string.Empty;

        /// <summary>
        /// Personal Access Token para autenticação (NÃO deve ser hardcoded).
        /// Deve vir do Secret Manager (dev) ou Environment Variables (prod).
        /// </summary>
        [Required(ErrorMessage = "O Personal Access Token é obrigatório")]
        public string PersonalAccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Versão da API REST do Azure DevOps.
        /// </summary>
        public string ApiVersion { get; set; } = "7.1";

        /// <summary>
        /// Timeout para requisições HTTP (em segundos).
        /// </summary>
        [Range(5, 300, ErrorMessage = "O timeout deve estar entre 5 e 300 segundos")]
        public int TimeoutSeconds { get; set; } = 30;
    }
}

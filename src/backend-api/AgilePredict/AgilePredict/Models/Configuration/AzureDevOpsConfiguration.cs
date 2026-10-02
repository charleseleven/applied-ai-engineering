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
        /// Organização padrão no Azure DevOps, usada quando a requisição não informa uma
        /// organização explícita. Como o Scrum Master pode analisar sprints de organizações
        /// e projetos diferentes (ex.: "inpart", "eleven11C", ou qualquer outra à qual tenha
        /// acesso), a organização/projeto normalmente vêm por requisição (via URL do Azure
        /// Boards ou campos explícitos) — este valor é apenas um fallback opcional de conveniência.
        /// </summary>
        public string? Organization { get; set; }

        /// <summary>
        /// Projeto padrão no Azure DevOps (fallback opcional; ver <see cref="Organization"/>).
        /// </summary>
        public string? Project { get; set; }

        /// <summary>
        /// Personal Access Token padrão (NÃO deve ser hardcoded; vem do Secret Manager em dev
        /// ou de Environment Variables em produção). Fallback opcional: a requisição pode enviar
        /// um PAT próprio para acessar outra organização à qual este token padrão não tem acesso.
        /// </summary>
        public string? PersonalAccessToken { get; set; }

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

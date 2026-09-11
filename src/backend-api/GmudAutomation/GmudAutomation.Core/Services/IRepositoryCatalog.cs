using GmudAutomation.Core.Models;

namespace GmudAutomation.Core.Services;

/// <summary>
/// Mapeia os repositórios conhecidos do cliente Contoso para sua categoria (US 3.1) e para o nome do
/// Azure App Service correspondente (US 2.1), seguindo o padrão contoso-portalcliente-{api|web}-{ambiente}-{cliente}.
/// </summary>
public interface IRepositoryCatalog
{
    bool TryGetKind(string repositoryName, out RepositoryKind kind);

    /// <summary>Retorna false para repositórios sem Azure App Service correspondente (ex: Database).</summary>
    bool TryGetSiteName(string repositoryName, string environmentSuffix, string clientSuffix, out string siteName);

    /// <summary>
    /// Resolve a tag curta usada pelo time nos comentários (ex: "API", "WEB", "Database", sem o prefixo
    /// "Contoso.PortalCliente.") para o nome canônico do repositório correspondente.
    /// </summary>
    bool TryGetRepositoryNameByTag(string tag, out string repositoryName);
}

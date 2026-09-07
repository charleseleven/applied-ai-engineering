using GmudAutomation.Core.Models;

namespace GmudAutomation.Core.Services;

/// <inheritdoc cref="IRepositoryCatalog"/>
public sealed class RepositoryCatalog : IRepositoryCatalog
{
    // Gerenciador de Jobs só tem instância de produção conhecida (confirmado pelo usuário em 07/09).
    private const string GerenciadorJobsProdSiteName = "contoso-integracao-gerenciadorjobs-prd-01";

    private static readonly Dictionary<string, RepositoryKind> KindsByRepositoryName = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Contoso.PortalCliente.API"] = RepositoryKind.Api,
        ["Contoso.PortalCliente.WEB"] = RepositoryKind.Web,
        ["Contoso.Integracao.GerenciadorJobs"] = RepositoryKind.Backend,
        ["Contoso.Legado.Database"] = RepositoryKind.Database
    };

    public bool TryGetKind(string repositoryName, out RepositoryKind kind) =>
        KindsByRepositoryName.TryGetValue(repositoryName, out kind);

    public bool TryGetSiteName(string repositoryName, string environmentSuffix, string clientSuffix, out string siteName)
    {
        siteName = string.Empty;

        if (!TryGetKind(repositoryName, out var kind))
        {
            return false;
        }

        siteName = kind switch
        {
            RepositoryKind.Api => $"contoso-portalcliente-api-{environmentSuffix}-{clientSuffix}",
            RepositoryKind.Web => $"contoso-portalcliente-web-{environmentSuffix}-{clientSuffix}",
            RepositoryKind.Backend => GerenciadorJobsProdSiteName,
            _ => string.Empty
        };

        // Database não roda em Azure App Service — não há tag de imagem a consultar.
        return kind != RepositoryKind.Database;
    }
}

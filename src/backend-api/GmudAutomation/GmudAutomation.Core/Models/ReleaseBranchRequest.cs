namespace GmudAutomation.Core.Models;

/// <summary>
/// Parâmetros para a criação da branch de release e o merge sequencial das features (US 2.1).
/// <paramref name="Environment"/> é o rótulo usado no NOME da branch (ex: "PROD", "GMUD"), enquanto
/// <paramref name="SiteName"/> é o nome real do Azure App Service consultado para a tag em execução —
/// na Contoso eles não são a mesma coisa (o site segue o padrão contoso-portalcliente-{api|web}-{ambiente}-{cliente}).
/// </summary>
public sealed record ReleaseBranchRequest(
    string Environment,
    string ClientName,
    DateOnly ReleaseDate,
    string RepositoryName,
    string SiteName,
    IReadOnlyList<string> FeatureBranchNames);

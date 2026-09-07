using GmudAutomation.Core.Client;
using GmudAutomation.Core.Models;
using GmudAutomation.Core.Services;

// Organização real do cliente: quando reconhecida na URL informada, o processo executado é o
// próprio propósito deste projeto — Automação e Orquestração do Processo de GMUD.
const string ContosoOrganization = "contoso";

// Mapeamento conhecido do cliente Contoso: Boards e Repos/Pipelines vivem em Team Projects diferentes
// dentro da mesma organização. Usado como padrão quando --repos-project não é informado.
const string ContosoReposProject = "Contoso Repositorios";

var urlParser = new AzureBoardsUrlParser();
var options = CliOptions.Parse(args, urlParser);
if (options is null)
{
    PrintUsage();
    return 1;
}

var reposProject = options.ReposProject
    ?? (string.Equals(options.Organization, ContosoOrganization, StringComparison.OrdinalIgnoreCase) ? ContosoReposProject : options.BoardsProject);

if (string.Equals(options.Organization, ContosoOrganization, StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine(
        $"Organização \"{ContosoOrganization}\" reconhecida — executando o processo de Automação e Orquestração do Processo de GMUD.");
}
else
{
    Console.WriteLine($"Executando para a organização informada: \"{options.Organization}\" (projeto \"{options.BoardsProject}\").");
}

if (!string.Equals(reposProject, options.BoardsProject, StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine($"Boards: \"{options.BoardsProject}\" | Repos/Pipelines: \"{reposProject}\".");
}

var patEnvironmentVariable = ResolvePatEnvironmentVariableName(options.Organization);
var personalAccessToken = Environment.GetEnvironmentVariable(patEnvironmentVariable);
if (string.IsNullOrWhiteSpace(personalAccessToken))
{
    Console.Error.WriteLine($"PAT não configurado. Defina a variável de ambiente {patEnvironmentVariable}.");
    return 1;
}

using var httpClient = new HttpClient();
var client = new AzureDevOpsClient(httpClient, new AzureDevOpsClientOptions
{
    Organization = options.Organization,
    BoardsProject = options.BoardsProject,
    ReposProject = reposProject,
    PersonalAccessToken = personalAccessToken
});

var linkParser = new WorkItemLinkParser();
var hierarchyService = new GmudHierarchyService(client, linkParser);
var tagExtractor = new ProjectTagExtractor();
var technicalImpactService = new TechnicalImpactService(client, tagExtractor);
var formatter = new TechnicalImpactReportFormatter();
var publisher = new GmudTechnicalImpactPublisher(technicalImpactService, formatter, client);
var orchestrator = new GmudAutomationOrchestrator(hierarchyService, publisher);

var repositoryCatalog = new RepositoryCatalog();
var releaseBranchNameBuilder = new ReleaseBranchNameBuilder();
using var appServiceHttpClient = new HttpClient();
var appServiceClient = new AppServiceClient(
    appServiceHttpClient,
    new AppServiceClientOptions { SubscriptionId = options.SubscriptionId },
    new AzureCliAccessTokenProvider());
var integrationBranchService = new IntegrationBranchService(appServiceClient, client, releaseBranchNameBuilder);
var pullRequestOrchestrationService = new PullRequestOrchestrationService(client);

try
{
    switch (options.Command)
    {
        case "hierarchy":
        {
            var gmudRequest = await hierarchyService.BuildHierarchyAsync(options.WorkItemId);
            PrintHierarchy(gmudRequest);
            return 0;
        }

        case "impact":
        {
            var impacts = await technicalImpactService.IdentifyTechnicalImpactAsync(options.WorkItemId);
            Console.WriteLine(formatter.FormatComment(impacts));
            Console.WriteLine();
            Console.WriteLine("(somente leitura — nenhum comentário foi publicado no card; use 'publish-impact' para isso)");
            return 0;
        }

        case "add-comment":
        {
            if (!options.ConfirmedWrite)
            {
                return RequireConfirmWrite();
            }

            if (string.IsNullOrWhiteSpace(options.CommentText))
            {
                Console.Error.WriteLine("Informe o texto do comentário com --text \"...\".");
                return 1;
            }

            await client.AddWorkItemCommentAsync(options.WorkItemId, options.CommentText);
            Console.WriteLine($"Comentário de teste publicado no work item #{options.WorkItemId}.");
            return 0;
        }

        case "publish-impact":
        {
            if (!options.ConfirmedWrite)
            {
                return RequireConfirmWrite();
            }

            var impacts = await publisher.AnalyzeAndPublishAsync(options.WorkItemId);
            Console.WriteLine($"Comentário publicado no work item #{options.WorkItemId} com {impacts.Count} impacto(s) identificado(s).");
            return 0;
        }

        case "run":
        {
            if (!options.ConfirmedWrite)
            {
                return RequireConfirmWrite();
            }

            var gmudRequest = await orchestrator.RunAsync(options.WorkItemId);
            PrintHierarchy(gmudRequest);
            Console.WriteLine();
            Console.WriteLine(
                $"Processo completo: impacto técnico identificado e publicado em {gmudRequest.ChildWorkItems.Count} work item(ns) filho(s).");
            return 0;
        }

        case "release-branch":
        {
            if (!options.ConfirmedWrite)
            {
                return RequireConfirmWrite();
            }

            var environment = ResolveEnvironment(options.Ambiente);
            if (environment is null || string.IsNullOrWhiteSpace(options.Cliente))
            {
                Console.Error.WriteLine("Informe --ambiente <gmud|prd> e --cliente <sufixo> para este comando.");
                return 1;
            }

            var branchesByRepository = await CollectRepositoryBranchesAsync(hierarchyService, technicalImpactService, options.WorkItemId);
            if (branchesByRepository.Count == 0)
            {
                Console.WriteLine("Nenhum projeto/branch identificado nos comentários dos work items filhos. Nada para fazer.");
                return 0;
            }

            foreach (var (repositoryName, featureBranches) in branchesByRepository)
            {
                if (!repositoryCatalog.TryGetSiteName(repositoryName, environment.Value.SiteSuffix, options.Cliente, out var siteName))
                {
                    Console.WriteLine($"[{repositoryName}] Sem Azure App Service correspondente (ex: Database) — trate a branch de release manualmente.");
                    continue;
                }

                Console.WriteLine($"[{repositoryName}] Criando branch de release a partir da tag em execução em \"{siteName}\"...");
                try
                {
                    var releaseBranch = await integrationBranchService.CreateReleaseBranchAsync(new ReleaseBranchRequest(
                        Environment: environment.Value.BranchLabel,
                        ClientName: options.Cliente,
                        ReleaseDate: DateOnly.FromDateTime(DateTime.Today),
                        RepositoryName: repositoryName,
                        SiteName: siteName,
                        FeatureBranchNames: featureBranches));

                    Console.WriteLine($"[{repositoryName}] Branch \"{releaseBranch}\" criada — {featureBranches.Count} feature branch(es) mesclada(s).");
                }
                catch (MergeConflictException ex)
                {
                    Console.Error.WriteLine($"[{repositoryName}] CONFLITO DE MERGE — resolução manual necessária:{Environment.NewLine}{ex.Message}");
                    return 1;
                }
            }

            return 0;
        }

        case "pull-requests":
        {
            if (!options.ConfirmedWrite)
            {
                return RequireConfirmWrite();
            }

            var environment = ResolveEnvironment(options.Ambiente);
            if (environment is null || string.IsNullOrWhiteSpace(options.Cliente))
            {
                Console.Error.WriteLine("Informe --ambiente <gmud|prd> e --cliente <sufixo> para este comando.");
                return 1;
            }

            var targetBranch = string.IsNullOrWhiteSpace(options.TargetBranch) ? "main" : options.TargetBranch;
            var branchesByRepository = await CollectRepositoryBranchesAsync(hierarchyService, technicalImpactService, options.WorkItemId);
            if (branchesByRepository.Count == 0)
            {
                Console.WriteLine("Nenhum projeto/branch identificado nos comentários dos work items filhos. Nada para fazer.");
                return 0;
            }

            var releaseBranchName = releaseBranchNameBuilder.Build(environment.Value.BranchLabel, options.Cliente, DateOnly.FromDateTime(DateTime.Today));
            var requests = new List<RepositoryPullRequestRequest>();

            foreach (var (repositoryName, featureBranches) in branchesByRepository)
            {
                if (!repositoryCatalog.TryGetKind(repositoryName, out var kind))
                {
                    Console.WriteLine($"[{repositoryName}] Repositório não catalogado — pulei a criação de PR.");
                    continue;
                }

                // Database não roda em App Service (sem branch de release) — a PR sai direto da feature branch identificada.
                var sourceBranch = kind == RepositoryKind.Database ? featureBranches[0] : releaseBranchName;
                requests.Add(new RepositoryPullRequestRequest(
                    repositoryName, kind, sourceBranch, targetBranch,
                    Title: $"GMUD #{options.WorkItemId} — {repositoryName}",
                    Description: $"PR gerada automaticamente pela automação de GMUD para o work item #{options.WorkItemId}."));
            }

            var outcomes = await pullRequestOrchestrationService.OrchestrateAsync(requests);
            foreach (var outcome in outcomes)
            {
                var status = outcome.Completed
                    ? $"completada (buildId: {(outcome.BuildId?.ToString() ?? "não capturado")})"
                    : "criada como Draft/Ativa (aguardando aprovação manual do DBA)";
                Console.WriteLine($"[{outcome.RepositoryName}] PR #{outcome.PullRequestId} {status} — {outcome.Url}");
            }

            return 0;
        }

        default:
            PrintUsage();
            return 1;
    }
}
catch (HttpRequestException ex)
{
    Console.Error.WriteLine($"Falha ao comunicar com o Azure DevOps: {ex.Message}");
    return 1;
}

static async Task<Dictionary<string, List<string>>> CollectRepositoryBranchesAsync(
    IGmudHierarchyService hierarchyService, ITechnicalImpactService technicalImpactService, int gmudWorkItemId)
{
    var hierarchy = await hierarchyService.BuildHierarchyAsync(gmudWorkItemId);
    var branchesByRepository = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

    foreach (var child in hierarchy.ChildWorkItems)
    {
        var impacts = await technicalImpactService.IdentifyTechnicalImpactAsync(child.Id);
        foreach (var impact in impacts)
        {
            if (!branchesByRepository.TryGetValue(impact.ProjectName, out var branches))
            {
                branches = [];
                branchesByRepository[impact.ProjectName] = branches;
            }

            if (!branches.Contains(impact.BranchName, StringComparer.OrdinalIgnoreCase))
            {
                branches.Add(impact.BranchName);
            }
        }
    }

    return branchesByRepository;
}

static (string SiteSuffix, string BranchLabel)? ResolveEnvironment(string? ambiente) => ambiente?.Trim().ToLowerInvariant() switch
{
    "gmud" => ("gmud", "GMUD"),
    "prd" or "prod" => ("prd", "PROD"),
    _ => null
};

static int RequireConfirmWrite()
{
    Console.Error.WriteLine("Esse comando ESCREVE no Azure Boards. Informe também --confirm-write para prosseguir.");
    return 1;
}

static void PrintHierarchy(GmudAutomation.Core.Models.GmudRequest gmudRequest)
{
    Console.WriteLine($"GMUD #{gmudRequest.Id}: {gmudRequest.Title}");
    Console.WriteLine($"Work items filhos ({gmudRequest.ChildWorkItems.Count}):");
    foreach (var child in gmudRequest.ChildWorkItems)
    {
        Console.WriteLine($"  - #{child.Id} [{child.State}] {child.Title}");
    }
}

static string ResolvePatEnvironmentVariableName(string organization) =>
    string.Equals(organization, CliOptions.DefaultOrganization, StringComparison.OrdinalIgnoreCase)
        ? "AZURE_DEVOPS_EXT_PAT"
        : $"AZURE_DEVOPS_PAT_{Sanitize(organization)}";

static string Sanitize(string value)
{
    var chars = value.ToUpperInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray();
    return new string(chars);
}

static void PrintUsage()
{
    Console.WriteLine(
        $"""
        Uso: dotnet run -- <comando> (--url <url-do-work-item> | --work-item <id> [--org <organização>] [--project <projeto>]) [opções]

        A organização/projeto/ID podem ser informados de duas formas:
          --url <url>                    URL do work item no Azure Boards (ex: https://dev.azure.com/contoso/PortalCliente/_workitems/edit/123)
          --work-item/--org/--project    informados manualmente (padrão: {CliOptions.DefaultOrganization} / {CliOptions.DefaultProject})

        Se a organização resolvida for "{ContosoOrganization}", o processo real de Automação e Orquestração da GMUD é reconhecido.
        Para qualquer outra organização, o mesmo processo roda normalmente para a organização informada.

        Opções:
          --repos-project <projeto>   Team Project dos repositórios/pipelines, se diferente do de Boards
                                       (para "{ContosoOrganization}" o padrão já é "{ContosoReposProject}")
          --ambiente <gmud|prd>        Ambiente da GMUD (obrigatório para 'release-branch'/'pull-requests')
          --cliente <sufixo>           Sufixo do cliente no nome do App Service, ex: "cli1", "cli2" (idem)
          --target <branch>            Branch de destino da PR em 'pull-requests' (padrão: "main")
          --subscription-id <id>       Subscription do Azure App Service (padrão: a da Contoso, já configurada)
          --text <texto>               Texto do comentário (obrigatório para 'add-comment')
          --confirm-write              Necessário para comandos que escrevem no Azure Boards/Repos

        Comandos:
          hierarchy        US 1.1 — mapeia a hierarquia da GMUD (somente leitura)
          impact           US 1.2 — identifica impacto técnico e valida branches (somente leitura)
          add-comment      Publica um comentário arbitrário no card (uso: dados de teste)
          publish-impact   US 1.2 — roda 'impact' e publica o resultado como comentário no card
          run              Processo completo Feature 1: hierarquia + impacto técnico publicado em cada filho
          release-branch   US 2.1 — cria a branch de release e mescla as features identificadas (--ambiente/--cliente)
          pull-requests    US 3.1 — abre as PRs (API/WEB/Backend completadas; Database só criada) (--ambiente/--cliente)
        """);
}

internal sealed class CliOptions
{
    // Sandbox padrão para uso sem --url (ex: validação em ambiente que não é o do cliente).
    public const string DefaultOrganization = "eleven11C";
    public const string DefaultProject = "Applied AI Engineering";

    // Subscription do Azure onde vivem os App Services da Contoso (confirmada em 07/09) — usada como
    // padrão para 'release-branch'/'pull-requests', que só fazem sentido para a organização "contoso".
    public const string DefaultSubscriptionId = "00000000-0000-0000-0000-000000000000";

    public required string Command { get; init; }
    public required int WorkItemId { get; init; }
    public required string Organization { get; init; }
    public required string BoardsProject { get; init; }
    public string? ReposProject { get; init; }
    public bool ConfirmedWrite { get; init; }
    public string? CommentText { get; init; }
    public string? Ambiente { get; init; }
    public string? Cliente { get; init; }
    public string? TargetBranch { get; init; }
    public required string SubscriptionId { get; init; }

    public static CliOptions? Parse(string[] args, IAzureBoardsUrlParser urlParser)
    {
        if (args.Length == 0)
        {
            return null;
        }

        var command = args[0];
        int? workItemId = null;
        var organization = DefaultOrganization;
        var project = DefaultProject;
        string? reposProject = null;
        var confirmedWrite = false;
        string? commentText = null;
        string? url = null;
        string? ambiente = null;
        string? cliente = null;
        string? targetBranch = null;
        var subscriptionId = DefaultSubscriptionId;

        for (var i = 1; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--work-item" when i + 1 < args.Length && int.TryParse(args[++i], out var id):
                    workItemId = id;
                    break;
                case "--org" when i + 1 < args.Length:
                    organization = args[++i];
                    break;
                case "--project" when i + 1 < args.Length:
                    project = args[++i];
                    break;
                case "--repos-project" when i + 1 < args.Length:
                    reposProject = args[++i];
                    break;
                case "--url" when i + 1 < args.Length:
                    url = args[++i];
                    break;
                case "--text" when i + 1 < args.Length:
                    commentText = args[++i];
                    break;
                case "--ambiente" when i + 1 < args.Length:
                    ambiente = args[++i];
                    break;
                case "--cliente" when i + 1 < args.Length:
                    cliente = args[++i];
                    break;
                case "--target" when i + 1 < args.Length:
                    targetBranch = args[++i];
                    break;
                case "--subscription-id" when i + 1 < args.Length:
                    subscriptionId = args[++i];
                    break;
                case "--confirm-write":
                    confirmedWrite = true;
                    break;
            }
        }

        if (url is not null)
        {
            if (!urlParser.TryParse(url, out var reference))
            {
                return null;
            }

            organization = reference.Organization;
            project = reference.Project;
            workItemId = reference.WorkItemId;
        }

        if (workItemId is null)
        {
            return null;
        }

        return new CliOptions
        {
            Command = command,
            WorkItemId = workItemId.Value,
            Organization = organization,
            BoardsProject = project,
            ReposProject = reposProject,
            ConfirmedWrite = confirmedWrite,
            CommentText = commentText,
            Ambiente = ambiente,
            Cliente = cliente,
            TargetBranch = targetBranch,
            SubscriptionId = subscriptionId
        };
    }
}

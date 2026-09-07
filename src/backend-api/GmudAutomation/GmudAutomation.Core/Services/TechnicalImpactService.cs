using GmudAutomation.Core.Client;
using GmudAutomation.Core.Models;

namespace GmudAutomation.Core.Services;

/// <summary>
/// US 1.2: varre os comentários de um work item filho (Task 1), identifica os projetos alterados
/// via as tags padrão do time (Task 2) e valida a existência da branch feature/us_&lt;numero&gt;
/// no repositório correspondente a cada projeto (Task 3).
/// </summary>
public sealed class TechnicalImpactService : ITechnicalImpactService
{
    private readonly IAzureDevOpsClient _client;
    private readonly IProjectTagExtractor _tagExtractor;

    public TechnicalImpactService(IAzureDevOpsClient client, IProjectTagExtractor tagExtractor)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _tagExtractor = tagExtractor ?? throw new ArgumentNullException(nameof(tagExtractor));
    }

    public async Task<IReadOnlyList<TechnicalImpact>> IdentifyTechnicalImpactAsync(int workItemId, CancellationToken cancellationToken = default)
    {
        var comments = await _client.GetWorkItemCommentsAsync(workItemId, cancellationToken);
        var branchName = $"feature/us_{workItemId}";

        var results = new List<TechnicalImpact>();
        var seenProjects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var comment in comments)
        {
            foreach (var projectName in _tagExtractor.ExtractProjectTags(comment.Text))
            {
                if (!seenProjects.Add(projectName))
                {
                    continue;
                }

                var branchExists = await _client.BranchExistsAsync(projectName, branchName, cancellationToken);
                results.Add(new TechnicalImpact(projectName, branchName, branchExists));
            }
        }

        return results;
    }
}

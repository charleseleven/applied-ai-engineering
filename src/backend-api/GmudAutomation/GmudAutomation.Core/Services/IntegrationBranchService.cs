using GmudAutomation.Core.Client;
using GmudAutomation.Core.Models;

namespace GmudAutomation.Core.Services;

/// <inheritdoc cref="IIntegrationBranchService"/>
public sealed class IntegrationBranchService : IIntegrationBranchService
{
    private readonly IAppServiceClient _appServiceClient;
    private readonly IAzureDevOpsClient _azureDevOpsClient;
    private readonly IReleaseBranchNameBuilder _nameBuilder;

    public IntegrationBranchService(IAppServiceClient appServiceClient, IAzureDevOpsClient azureDevOpsClient, IReleaseBranchNameBuilder nameBuilder)
    {
        _appServiceClient = appServiceClient ?? throw new ArgumentNullException(nameof(appServiceClient));
        _azureDevOpsClient = azureDevOpsClient ?? throw new ArgumentNullException(nameof(azureDevOpsClient));
        _nameBuilder = nameBuilder ?? throw new ArgumentNullException(nameof(nameBuilder));
    }

    public async Task<string> CreateReleaseBranchAsync(ReleaseBranchRequest request, CancellationToken cancellationToken = default)
    {
        var releaseBranchName = _nameBuilder.Build(request.Environment, request.ClientName, request.ReleaseDate);
        var currentTag = await _appServiceClient.GetCurrentTagAsync(request.SiteName, cancellationToken);
        await _azureDevOpsClient.CreateBranchAsync(request.RepositoryName, currentTag, releaseBranchName, cancellationToken);

        foreach (var featureBranch in request.FeatureBranchNames)
        {
            // Interrompe na primeira MergeConflictException — o AC exige que a execução pare para resolução manual do Tech Lead.
            await _azureDevOpsClient.MergeBranchesAsync(request.RepositoryName, featureBranch, releaseBranchName, cancellationToken);
        }

        return releaseBranchName;
    }
}

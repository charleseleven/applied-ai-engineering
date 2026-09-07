using FluentAssertions;
using GmudAutomation.Core.Client;
using GmudAutomation.Core.Models;
using GmudAutomation.Core.Services;
using Moq;

namespace GmudAutomation.Tests.Services;

public class IntegrationBranchServiceTests
{
    private readonly Mock<IAppServiceClient> _appServiceClientMock = new();
    private readonly Mock<IAzureDevOpsClient> _azureDevOpsClientMock = new();
    private readonly Mock<IReleaseBranchNameBuilder> _nameBuilderMock = new();
    private readonly IntegrationBranchService _sut;

    public IntegrationBranchServiceTests()
    {
        _sut = new IntegrationBranchService(_appServiceClientMock.Object, _azureDevOpsClientMock.Object, _nameBuilderMock.Object);
    }

    private static ReleaseBranchRequest CreateRequest(params string[] featureBranches) => new(
        Environment: "PROD",
        ClientName: "Contoso",
        ReleaseDate: new DateOnly(2026, 9, 10),
        RepositoryName: "Contoso.PortalCliente.API",
        SiteName: "contoso-portalcliente-api-prd-cli1",
        FeatureBranchNames: featureBranches);

    [Fact]
    public async Task CreateReleaseBranchAsync_TresFeatureBranches_CriaABranchEMesclaAsTresSequencialmente()
    {
        var request = CreateRequest("feature/us_192", "feature/us_193", "feature/us_194");
        _nameBuilderMock.Setup(b => b.Build("PROD", "Contoso", request.ReleaseDate)).Returns("release/GMUD_PROD_Contoso_10092026");
        _appServiceClientMock.Setup(a => a.GetCurrentTagAsync("contoso-portalcliente-api-prd-cli1", It.IsAny<CancellationToken>())).ReturnsAsync("25448");

        var result = await _sut.CreateReleaseBranchAsync(request);

        result.Should().Be("release/GMUD_PROD_Contoso_10092026");
        _azureDevOpsClientMock.Verify(c => c.CreateBranchAsync(
            "Contoso.PortalCliente.API", "25448", "release/GMUD_PROD_Contoso_10092026", It.IsAny<CancellationToken>()), Times.Once);

        foreach (var featureBranch in request.FeatureBranchNames)
        {
            _azureDevOpsClientMock.Verify(c => c.MergeBranchesAsync(
                "Contoso.PortalCliente.API", featureBranch, "release/GMUD_PROD_Contoso_10092026", It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Fact]
    public async Task CreateReleaseBranchAsync_MergeRetornaConflito_PropagaAMergeConflictExceptionEInterrompeAsProximasMerges()
    {
        var request = CreateRequest("feature/us_192", "feature/us_193");
        _nameBuilderMock.Setup(b => b.Build(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>())).Returns("release/GMUD_PROD_Contoso_10092026");
        _appServiceClientMock.Setup(a => a.GetCurrentTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("25448");
        _azureDevOpsClientMock
            .Setup(c => c.MergeBranchesAsync("Contoso.PortalCliente.API", "feature/us_192", "release/GMUD_PROD_Contoso_10092026", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MergeConflictException("feature/us_192", "release/GMUD_PROD_Contoso_10092026", ["Arquivo.cs"]));

        var act = () => _sut.CreateReleaseBranchAsync(request);

        await act.Should().ThrowAsync<MergeConflictException>().WithMessage("*Arquivo.cs*");
        _azureDevOpsClientMock.Verify(c => c.MergeBranchesAsync(
            "Contoso.PortalCliente.API", "feature/us_193", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

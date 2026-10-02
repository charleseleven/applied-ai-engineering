using AgilePredict.Services;
using Xunit;

namespace AgilePredict.Tests.Services.Flow
{
    public class AzureDevOpsBoardsUrlParserTests
    {
        private readonly AzureDevOpsBoardsUrlParser _parser = new();

        [Fact]
        public void Parse_ModernWorkItemUrl_ExtractsOrganizationAndProject()
        {
            var result = _parser.Parse("https://dev.azure.com/eleven11C/Applied%20AI%20Engineering/_workitems/edit/211");

            Assert.Equal("eleven11C", result.Organization);
            Assert.Equal("Applied AI Engineering", result.Project);
        }

        [Fact]
        public void Parse_ModernUrlWithAccentedProjectName_DecodesCorrectly()
        {
            var result = _parser.Parse("https://dev.azure.com/inpart/Inpart%20Sa%C3%BAde%20Projetos/_sprints/taskboard/Time/Inpart%20Sa%C3%BAde%20Projetos/Sprint%201");

            Assert.Equal("inpart", result.Organization);
            Assert.Equal("Inpart Saúde Projetos", result.Project);
        }

        [Fact]
        public void Parse_LegacyVisualStudioComUrl_ExtractsOrganizationAndProject()
        {
            var result = _parser.Parse("https://minhaorg.visualstudio.com/MeuProjeto/_boards/board");

            Assert.Equal("minhaorg", result.Organization);
            Assert.Equal("MeuProjeto", result.Project);
        }

        [Fact]
        public void Parse_UrlWithOnlyOrganizationAndProject_Works()
        {
            var result = _parser.Parse("https://dev.azure.com/eleven11C/Applied%20AI%20Engineering");

            Assert.Equal("eleven11C", result.Organization);
            Assert.Equal("Applied AI Engineering", result.Project);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Parse_EmptyUrl_Throws(string? url)
        {
            Assert.Throws<ArgumentException>(() => _parser.Parse(url!));
        }

        [Fact]
        public void Parse_NonAzureDevOpsHost_Throws()
        {
            Assert.Throws<ArgumentException>(() => _parser.Parse("https://github.com/foo/bar"));
        }

        [Fact]
        public void Parse_NotAUrl_Throws()
        {
            Assert.Throws<ArgumentException>(() => _parser.Parse("Applied AI Engineering\\Sprint 1"));
        }

        [Fact]
        public void Parse_DevAzureComWithOnlyOrganization_Throws()
        {
            Assert.Throws<ArgumentException>(() => _parser.Parse("https://dev.azure.com/eleven11C"));
        }

        [Fact]
        public void TryExtractIterationPath_TaskboardUrl_JoinsRemainingSegmentsWithBackslash()
        {
            var iterationPath = _parser.TryExtractIterationPath(
                "https://dev.azure.com/inpart/Inpart%20Sa%C3%BAde%20Projetos/_sprints/taskboard/Time%20Implantacao/Inpart%20Sa%C3%BAde%20Projetos/2026-09-Sprint-17");

            Assert.Equal("Inpart Saúde Projetos\\2026-09-Sprint-17", iterationPath);
        }

        [Fact]
        public void TryExtractIterationPath_BacklogUrl_JoinsRemainingSegmentsWithBackslash()
        {
            var iterationPath = _parser.TryExtractIterationPath(
                "https://dev.azure.com/eleven11C/Applied%20AI%20Engineering/_sprints/backlog/Time/Applied%20AI%20Engineering/2026-09-Sprint-01");

            Assert.Equal("Applied AI Engineering\\2026-09-Sprint-01", iterationPath);
        }

        [Fact]
        public void TryExtractIterationPath_UrlWithoutSprintsSegment_ReturnsNull()
        {
            var iterationPath = _parser.TryExtractIterationPath(
                "https://dev.azure.com/eleven11C/Applied%20AI%20Engineering/_workitems/edit/211");

            Assert.Null(iterationPath);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        [InlineData("Applied AI Engineering\\Sprint 1")]
        public void TryExtractIterationPath_NotAUrlOrEmpty_ReturnsNullWithoutThrowing(string? url)
        {
            Assert.Null(_parser.TryExtractIterationPath(url!));
        }
    }
}

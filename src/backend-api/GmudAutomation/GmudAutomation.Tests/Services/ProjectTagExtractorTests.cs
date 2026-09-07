using FluentAssertions;
using GmudAutomation.Core.Services;

namespace GmudAutomation.Tests.Services;

public class ProjectTagExtractorTests
{
    private readonly ProjectTagExtractor _sut = new();

    [Theory]
    [InlineData("Alterado o projeto Contoso.PortalCliente.API para incluir o novo endpoint.", "Contoso.PortalCliente.API")]
    [InlineData("Ajuste realizado em Contoso.PortalCliente.WEB conforme solicitado.", "Contoso.PortalCliente.WEB")]
    [InlineData("Script criado em Contoso.PortalCliente.Database.", "Contoso.PortalCliente.Database")]
    public void ExtractProjectTags_ComentarioComUmaTagConhecida_RetornaONomeDoProjeto(string comentario, string projetoEsperado)
    {
        var result = _sut.ExtractProjectTags(comentario);

        result.Should().ContainSingle().Which.Should().Be(projetoEsperado);
    }

    [Fact]
    public void ExtractProjectTags_ComentarioComMultiplasTags_RetornaTodosOsProjetosDistintos()
    {
        const string comentario = "Alterações em Contoso.PortalCliente.API e Contoso.PortalCliente.Database para essa GMUD.";

        var result = _sut.ExtractProjectTags(comentario);

        result.Should().BeEquivalentTo(["Contoso.PortalCliente.API", "Contoso.PortalCliente.Database"]);
    }

    [Fact]
    public void ExtractProjectTags_ComentarioSemTagsConhecidas_RetornaListaVaziaSemLancarExcecao()
    {
        var act = () => _sut.ExtractProjectTags("Apenas um comentário informativo sem menção a projetos.");

        act.Should().NotThrow();
        act().Should().BeEmpty();
    }

    [Fact]
    public void ExtractProjectTags_ComentarioNulo_RetornaListaVaziaSemLancarExcecao()
    {
        var act = () => _sut.ExtractProjectTags(null);

        act.Should().NotThrow();
        act().Should().BeEmpty();
    }
}

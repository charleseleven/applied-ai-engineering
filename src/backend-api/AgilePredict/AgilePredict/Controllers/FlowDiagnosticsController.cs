using AgilePredict.Models.DTOs.Flow;
using AgilePredict.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgilePredict.Controllers
{
    /// <summary>
    /// Expõe o diagnóstico preditivo de gargalos de fluxo (US #211) para o painel executivo do
    /// Scrum Master (Task #215).
    /// </summary>
    [ApiController]
    [Route("api/flow-diagnostics")]
    [Produces("application/json")]
    public class FlowDiagnosticsController : ControllerBase
    {
        private readonly IFlowDiagnosticsOrchestrator _orchestrator;
        private readonly ILogger<FlowDiagnosticsController> _logger;

        public FlowDiagnosticsController(
            IFlowDiagnosticsOrchestrator orchestrator,
            ILogger<FlowDiagnosticsController> logger)
        {
            _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gera o relatório de diagnóstico preditivo (Cycle Time vs. baseline, alertas de gargalo
        /// por status/responsável e sugestões de mitigação geradas por IA) para a sprint informada.
        /// </summary>
        /// <param name="iterationPath">
        /// Caminho da iteração/sprint no Azure DevOps (ex.: "Applied AI Engineering\\Sprint 4").
        /// </param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <response code="200">Relatório gerado com sucesso.</response>
        /// <response code="400">Parâmetro iterationPath ausente ou inválido.</response>
        /// <response code="502">Falha ao coletar dados na API do Azure DevOps.</response>
        [HttpGet]
        [ProducesResponseType(typeof(FlowDiagnosticReport), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<FlowDiagnosticReport>> GetDiagnostics(
            [FromQuery] string iterationPath,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(iterationPath))
            {
                return BadRequest(new { message = "O parâmetro 'iterationPath' é obrigatório." });
            }

            try
            {
                var report = await _orchestrator.GenerateReportAsync(iterationPath, cancellationToken);
                return Ok(report);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Falha ao coletar dados de Work Items para a iteração {IterationPath}", iterationPath);
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    message = "Não foi possível coletar os dados de Work Items na API do Azure DevOps."
                });
            }
        }
    }
}

using AgilePredict.Models.DTOs.SprintRisk;
using AgilePredict.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgilePredict.Controllers
{
    /// <summary>
    /// Expõe o relatório formatado do bot preditivo de risco de Sprint (PBI #160/#163, Feature
    /// #147) para o dashboard executivo do frontend (Task #165).
    /// </summary>
    [ApiController]
    [Route("api/sprint-risk-bot")]
    [Produces("application/json")]
    public class SprintRiskBotController : ControllerBase
    {
        private readonly ISprintRiskBotOrchestrator _orchestrator;

        public SprintRiskBotController(ISprintRiskBotOrchestrator orchestrator)
        {
            _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        }

        /// <summary>
        /// Gera o relatório preditivo de risco (capacidade diária histórica do time vs. Story
        /// Points restantes) para todas as Sprints ativas, com diagnóstico textual gerado por IA.
        /// </summary>
        /// <response code="200">Relatório gerado com sucesso.</response>
        [HttpGet("report")]
        [ProducesResponseType(typeof(SprintRiskReport), StatusCodes.Status200OK)]
        public async Task<ActionResult<SprintRiskReport>> GetReport(CancellationToken cancellationToken)
        {
            var report = await _orchestrator.GenerateReportAsync(cancellationToken);
            return Ok(report);
        }
    }
}

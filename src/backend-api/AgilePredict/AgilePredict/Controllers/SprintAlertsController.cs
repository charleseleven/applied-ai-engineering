using AgilePredict.Data;
using AgilePredict.Models;
using AgilePredict.Models.DTOs;
using AgilePredict.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgilePredict.Controllers
{
    /// <summary>
    /// Expõe os alertas gerados pelo agente autônomo de monitoramento de Sprints (PBI #150)
    /// e o resumo de saúde usado pelo painel preditivo (PBI #230)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SprintAlertsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISprintHealthAnalyzer _analyzer;

        public SprintAlertsController(AppDbContext context, ISprintHealthAnalyzer analyzer)
        {
            _context = context;
            _analyzer = analyzer;
        }

        /// <summary>
        /// Lista os alertas gerados pelo agente, mais recentes primeiro
        /// </summary>
        /// <param name="onlyUnacknowledged">Se true, retorna só os ainda não reconhecidos</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SprintHealthAlert>>> GetAlerts(
            [FromQuery] bool onlyUnacknowledged = false)
        {
            var query = _context.SprintHealthAlerts.AsQueryable();
            if (onlyUnacknowledged)
            {
                query = query.Where(a => !a.Acknowledged);
            }

            return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// Resumo de saúde (completion% vs tempo decorrido%) de cada Sprint ativa
        /// </summary>
        [HttpGet("health-summary")]
        public async Task<ActionResult<IEnumerable<SprintHealthSummary>>> GetHealthSummary(
            CancellationToken cancellationToken)
        {
            return Ok(await _analyzer.GetHealthSummaryAsync(cancellationToken));
        }

        /// <summary>
        /// Marca um alerta como reconhecido (não aparece mais como pendente)
        /// </summary>
        [HttpPost("{id}/acknowledge")]
        public async Task<IActionResult> Acknowledge(int id)
        {
            var alert = await _context.SprintHealthAlerts.FindAsync(id);
            if (alert == null)
            {
                return NotFound();
            }

            alert.Acknowledged = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

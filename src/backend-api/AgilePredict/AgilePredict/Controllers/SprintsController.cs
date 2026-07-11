using AgilePredict.Data;
using AgilePredict.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgilePredict.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de Sprints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SprintsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SprintsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retorna todas as sprints
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sprint>>> GetSprints()
        {
            return await _context.Sprints
                .Include(s => s.Project)
                .Include(s => s.Tasks)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna uma sprint específica por ID
        /// </summary>
        /// <param name="id">ID da sprint</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<Sprint>> GetSprint(int id)
        {
            var sprint = await _context.Sprints
                .Include(s => s.Project)
                .Include(s => s.Tasks)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sprint == null)
            {
                return NotFound();
            }

            return sprint;
        }

        /// <summary>
        /// Cria uma nova sprint
        /// </summary>
        /// <param name="sprint">Dados da sprint</param>
        [HttpPost]
        public async Task<ActionResult<Sprint>> PostSprint(Sprint sprint)
        {
            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSprint), new { id = sprint.Id }, sprint);
        }

        /// <summary>
        /// Atualiza uma sprint existente
        /// </summary>
        /// <param name="id">ID da sprint</param>
        /// <param name="sprint">Dados atualizados da sprint</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSprint(int id, Sprint sprint)
        {
            if (id != sprint.Id)
            {
                return BadRequest();
            }

            sprint.UpdatedAt = DateTime.UtcNow;
            _context.Entry(sprint).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await SprintExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Deleta uma sprint
        /// </summary>
        /// <param name="id">ID da sprint</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSprint(int id)
        {
            var sprint = await _context.Sprints.FindAsync(id);
            if (sprint == null)
            {
                return NotFound();
            }

            _context.Sprints.Remove(sprint);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> SprintExists(int id)
        {
            return await _context.Sprints.AnyAsync(e => e.Id == id);
        }
    }
}

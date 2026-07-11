using AgilePredict.Data;
using AgilePredict.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgilePredict.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de Tasks
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retorna todas as tasks
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectTask>>> GetTasks()
        {
            return await _context.ProjectTasks
                .Include(t => t.Sprint)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna uma task específica por ID
        /// </summary>
        /// <param name="id">ID da task</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectTask>> GetTask(int id)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Sprint)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            return task;
        }

        /// <summary>
        /// Cria uma nova task
        /// </summary>
        /// <param name="task">Dados da task</param>
        [HttpPost]
        public async Task<ActionResult<ProjectTask>> PostTask(ProjectTask task)
        {
            // Valida se a Sprint existe
            var sprintExists = await _context.Sprints.AnyAsync(s => s.Id == task.SprintId);
            if (!sprintExists)
            {
                return BadRequest(new { error = $"Sprint com ID {task.SprintId} não existe." });
            }

            _context.ProjectTasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        /// <summary>
        /// Atualiza uma task existente
        /// </summary>
        /// <param name="id">ID da task</param>
        /// <param name="task">Dados atualizados da task</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTask(int id, ProjectTask task)
        {
            if (id != task.Id)
            {
                return BadRequest();
            }

            task.UpdatedAt = DateTime.UtcNow;
            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TaskExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Deleta uma task
        /// </summary>
        /// <param name="id">ID da task</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.ProjectTasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> TaskExists(int id)
        {
            return await _context.ProjectTasks.AnyAsync(e => e.Id == id);
        }
    }
}

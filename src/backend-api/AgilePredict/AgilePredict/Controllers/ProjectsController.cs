using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgilePredict.Data;
using AgilePredict.Models;

namespace AgilePredict.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de Projects com suporte a criação aninhada
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retorna todos os projects
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            return await _context.Projects
                .Include(p => p.Sprints)
                    .ThenInclude(s => s.Tasks)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna um project específico por ID
        /// </summary>
        /// <param name="id">ID do project</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Sprints)
                    .ThenInclude(s => s.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return project;
        }

        /// <summary>
        /// Cria um novo project com suporte para Sprints e Tasks aninhados
        /// </summary>
        /// <param name="project">Dados do project</param>
        [HttpPost]
        public async Task<ActionResult<Project>> PostProject(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, new { id = project.Id });
        }

        /// <summary>
        /// Atualiza um project existente
        /// </summary>
        /// <param name="id">ID do project</param>
        /// <param name="project">Dados atualizados do project</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, Project project)
        {
            if (id != project.Id)
            {
                return BadRequest();
            }

            project.UpdatedAt = DateTime.UtcNow;
            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProjectExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Deleta um project
        /// </summary>
        /// <param name="id">ID do project</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ProjectExists(int id)
        {
            return await _context.Projects.AnyAsync(e => e.Id == id);
        }
    }
}

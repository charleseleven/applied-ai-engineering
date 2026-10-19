using AgilePredict.Data;
using AgilePredict.Models;
using AgilePredict.Models.DTOs.SprintRisk;
using AgilePredict.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgilePredict.Services
{
    /// <summary>
    /// Implementação da Task #161 (parte de coleta): consulta Sprints/Tasks de forma assíncrona
    /// via EF Core e materializa os dados em DTOs simples, desacoplados de qualquer entidade.
    /// </summary>
    public class SprintRiskDataSource : ISprintRiskDataSource
    {
        private const string ActiveStatus = "Active";

        private readonly AppDbContext _context;

        public SprintRiskDataSource(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IReadOnlyList<ActiveSprintSnapshot>> GetActiveSprintsAsync(CancellationToken cancellationToken = default)
        {
            var sprints = await _context.Sprints
                .Include(s => s.Tasks)
                .Where(s => s.Status == ActiveStatus)
                .ToListAsync(cancellationToken);

            return sprints.Select(ToSnapshot).ToList();
        }

        public async Task<IReadOnlyList<SprintHistoricalVelocitySample>> GetHistoricalVelocitySamplesAsync(
            int lookbackCount,
            string completedStatus,
            CancellationToken cancellationToken = default)
        {
            var sprints = await _context.Sprints
                .Include(s => s.Tasks)
                .Where(s => s.Status == completedStatus)
                .OrderByDescending(s => s.EndDate)
                .Take(lookbackCount)
                .ToListAsync(cancellationToken);

            return sprints
                .Select(s =>
                {
                    var durationDays = Math.Max(1.0, (s.EndDate - s.StartDate).TotalDays);
                    return new SprintHistoricalVelocitySample
                    {
                        SprintId = s.Id,
                        SprintTitle = s.Title,
                        DailyVelocity = SumCompletedPoints(s) / durationDays
                    };
                })
                .OrderBy(s => s.SprintId)
                .ToList();
        }

        private static ActiveSprintSnapshot ToSnapshot(Sprint sprint) => new()
        {
            SprintId = sprint.Id,
            SprintTitle = sprint.Title,
            TotalStoryPoints = sprint.TotalStoryPoints > 0 ? sprint.TotalStoryPoints : sprint.Tasks.Sum(t => t.StoryPoints),
            CompletedStoryPoints = SumCompletedPoints(sprint),
            StartDate = sprint.StartDate,
            EndDate = sprint.EndDate
        };

        private static int SumCompletedPoints(Sprint sprint) =>
            sprint.Tasks.Where(t => t.Status == "Done").Sum(t => t.StoryPoints);
    }
}

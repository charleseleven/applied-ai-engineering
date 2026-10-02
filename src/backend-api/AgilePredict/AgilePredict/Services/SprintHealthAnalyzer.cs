using AgilePredict.Data;
using AgilePredict.Models;
using AgilePredict.Models.Configuration;
using AgilePredict.Models.DTOs;
using AgilePredict.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AgilePredict.Services
{
    /// <summary>
    /// Implementação da Task #152 (Lógica de Coleta e Agregação): consulta o histórico
    /// transacional das Tasks via Entity Framework e calcula a saúde de cada Sprint ativa.
    /// </summary>
    public class SprintHealthAnalyzer : ISprintHealthAnalyzer
    {
        private readonly AppDbContext _context;
        private readonly SprintHealthMonitorConfiguration _configuration;
        private readonly ILogger<SprintHealthAnalyzer> _logger;

        public SprintHealthAnalyzer(
            AppDbContext context,
            IOptions<SprintHealthMonitorConfiguration> configuration,
            ILogger<SprintHealthAnalyzer> logger)
        {
            _context = context;
            _configuration = configuration.Value;
            _logger = logger;
        }

        public async Task<int> AnalyzeActiveSprintsAsync(CancellationToken cancellationToken = default)
        {
            var activeSprints = await _context.Sprints
                .Include(s => s.Tasks)
                .Where(s => s.Status == "Active")
                .ToListAsync(cancellationToken);

            var alertsCreated = 0;

            foreach (var sprint in activeSprints)
            {
                var (completionPercent, timeElapsedPercent) = CalculateHealth(sprint);
                var gap = timeElapsedPercent - completionPercent;

                if (gap < _configuration.RiskThresholdPercentPoints)
                {
                    continue;
                }

                var recentDuplicate = await _context.SprintHealthAlerts
                    .Where(a => a.SprintId == sprint.Id && !a.Acknowledged)
                    .Where(a => a.CreatedAt >= DateTime.UtcNow.AddHours(-24))
                    .AnyAsync(cancellationToken);

                if (recentDuplicate)
                {
                    continue;
                }

                var severity = gap >= _configuration.RiskThresholdPercentPoints * 2 ? "Critical" : "Warning";

                _context.SprintHealthAlerts.Add(new SprintHealthAlert
                {
                    SprintId = sprint.Id,
                    Severity = severity,
                    CompletionPercent = Math.Round(completionPercent, 1),
                    TimeElapsedPercent = Math.Round(timeElapsedPercent, 1),
                    Message = $"Sprint \"{sprint.Title}\" está em risco: {timeElapsedPercent:0}% do tempo decorrido, " +
                              $"mas apenas {completionPercent:0}% dos Story Points concluídos."
                });

                alertsCreated++;
                _logger.LogWarning(
                    "Gargalo detectado na Sprint #{SprintId} ({Title}): tempo decorrido {TimeElapsed:0}% vs concluído {Completion:0}%",
                    sprint.Id, sprint.Title, timeElapsedPercent, completionPercent);
            }

            if (alertsCreated > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return alertsCreated;
        }

        public async Task<IReadOnlyList<SprintHealthSummary>> GetHealthSummaryAsync(CancellationToken cancellationToken = default)
        {
            var activeSprints = await _context.Sprints
                .Include(s => s.Tasks)
                .Where(s => s.Status == "Active")
                .ToListAsync(cancellationToken);

            return activeSprints.Select(sprint =>
            {
                var (completionPercent, timeElapsedPercent) = CalculateHealth(sprint);
                var gap = timeElapsedPercent - completionPercent;

                return new SprintHealthSummary
                {
                    SprintId = sprint.Id,
                    SprintTitle = sprint.Title,
                    CompletionPercent = Math.Round(completionPercent, 1),
                    TimeElapsedPercent = Math.Round(timeElapsedPercent, 1),
                    IsAtRisk = gap >= _configuration.RiskThresholdPercentPoints
                };
            }).ToList();
        }

        private static (double CompletionPercent, double TimeElapsedPercent) CalculateHealth(Sprint sprint)
        {
            var totalPoints = sprint.TotalStoryPoints > 0
                ? sprint.TotalStoryPoints
                : sprint.Tasks.Sum(t => t.StoryPoints);

            var completedPoints = sprint.Tasks
                .Where(t => t.Status == "Done")
                .Sum(t => t.StoryPoints);

            var completionPercent = totalPoints > 0 ? completedPoints * 100.0 / totalPoints : 0;

            var totalDuration = (sprint.EndDate - sprint.StartDate).TotalMinutes;
            var elapsed = (DateTime.UtcNow - sprint.StartDate).TotalMinutes;
            var timeElapsedPercent = totalDuration > 0
                ? Math.Clamp(elapsed / totalDuration * 100, 0, 100)
                : 100;

            return (completionPercent, timeElapsedPercent);
        }
    }
}

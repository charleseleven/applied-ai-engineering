using AgilePredict.Data;
using AgilePredict.Models;
using AgilePredict.Models.DTOs;
using AgilePredict.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgilePredict.Services
{
    /// <summary>
    /// Implementação da camada de proteção/validação e persistência transacional
    /// para ações de automação executadas pela IA sobre Tasks.
    /// </summary>
    public class TaskAutomationService : ITaskAutomationService
    {
        private static readonly string[] ValidStatuses = ["To Do", "In Progress", "Done"];

        private readonly AppDbContext _context;
        private readonly ILogger<TaskAutomationService> _logger;

        public TaskAutomationService(AppDbContext context, ILogger<TaskAutomationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TaskActionResult> UpdateTaskStatusAsync(
            int taskId, string newStatus, CancellationToken cancellationToken = default)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Sprint)
                .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

            if (task == null)
            {
                return Failure(taskId, $"Task #{taskId} não encontrada.");
            }

            if (!ValidStatuses.Contains(newStatus))
            {
                return Failure(taskId,
                    $"Status \"{newStatus}\" inválido. Valores aceitos: {string.Join(", ", ValidStatuses)}.");
            }

            if (task.Sprint != null && task.Sprint.EndDate < DateTime.UtcNow)
            {
                return Failure(taskId,
                    $"A Sprint \"{task.Sprint.Title}\" já foi encerrada em " +
                    $"{task.Sprint.EndDate:dd/MM/yyyy} — não é permitido alterar tasks de sprints encerradas.");
            }

            var oldStatus = task.Status;

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                task.Status = newStatus;
                task.UpdatedAt = DateTime.UtcNow;

                _context.AiAuditLogs.Add(new AiAuditLog
                {
                    EntityType = "ProjectTask",
                    EntityId = taskId,
                    Action = "UpdateStatus",
                    OldValue = oldStatus,
                    NewValue = newStatus,
                    Source = "AI"
                });

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Falha ao persistir atualização de status da Task #{TaskId}", taskId);
                return Failure(taskId, "Erro interno ao persistir a alteração. Nenhuma mudança foi aplicada.");
            }

            _logger.LogInformation(
                "IA atualizou Task #{TaskId}: status \"{OldStatus}\" -> \"{NewStatus}\"", taskId, oldStatus, newStatus);

            return new TaskActionResult
            {
                Success = true,
                TaskId = taskId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Message = $"Task #{taskId} atualizada de \"{oldStatus}\" para \"{newStatus}\"."
            };
        }

        private static TaskActionResult Failure(int taskId, string message) => new()
        {
            Success = false,
            TaskId = taskId,
            Message = message
        };
    }
}

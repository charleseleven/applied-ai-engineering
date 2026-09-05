using AgilePredict.Models.DTOs;

namespace AgilePredict.Services.Interfaces
{
    /// <summary>
    /// Camada de domínio para ações de automação sobre Tasks decididas pela IA,
    /// com validação de regras de negócio e persistência transacional.
    /// </summary>
    public interface ITaskAutomationService
    {
        /// <summary>
        /// Atualiza o status de uma Task, validando regras de negócio antes de persistir
        /// e registrando a ação no log de auditoria.
        /// </summary>
        Task<TaskActionResult> UpdateTaskStatusAsync(int taskId, string newStatus, CancellationToken cancellationToken = default);
    }
}

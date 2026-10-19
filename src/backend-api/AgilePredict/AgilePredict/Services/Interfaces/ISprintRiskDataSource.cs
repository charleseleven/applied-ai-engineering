using AgilePredict.Models.DTOs.SprintRisk;

namespace AgilePredict.Services.Interfaces
{
    /// <summary>
    /// Acesso assíncrono (EF Core) aos dados de Sprints/Tasks necessários para o bot preditivo
    /// de risco (Task #161, AC: "consumir de forma assíncrona os dados do banco relacional via
    /// Entity Framework Core"). Mantido separado do calculador puro para isolar essa dependência.
    /// </summary>
    public interface ISprintRiskDataSource
    {
        Task<IReadOnlyList<ActiveSprintSnapshot>> GetActiveSprintsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Velocidade diária das últimas <paramref name="lookbackCount"/> Sprints cujo
        /// <c>Status</c> seja igual a <paramref name="completedStatus"/>, da mais recente para a
        /// mais antiga internamente, mas retornada ordenada da mais antiga para a mais recente.
        /// </summary>
        Task<IReadOnlyList<SprintHistoricalVelocitySample>> GetHistoricalVelocitySamplesAsync(
            int lookbackCount,
            string completedStatus,
            CancellationToken cancellationToken = default);
    }
}

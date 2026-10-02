using AgilePredict.Models.Configuration;
using AgilePredict.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AgilePredict.Services
{
    /// <summary>
    /// Implementação da Task #151 (Configuração do HostedService): agente autônomo que
    /// roda em background, sem depender de requisições HTTP, analisando periodicamente
    /// a saúde das Sprints ativas (PBI #150).
    /// </summary>
    public class SprintHealthMonitorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly SprintHealthMonitorConfiguration _configuration;
        private readonly ILogger<SprintHealthMonitorService> _logger;

        public SprintHealthMonitorService(
            IServiceScopeFactory scopeFactory,
            IOptions<SprintHealthMonitorConfiguration> configuration,
            ILogger<SprintHealthMonitorService> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Roda uma vez imediatamente ao iniciar, em vez de esperar o primeiro intervalo completo.
            await RunAnalysisAsync(stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromHours(_configuration.CheckIntervalHours));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await timer.WaitForNextTickAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                await RunAnalysisAsync(stoppingToken);
            }
        }

        private async Task RunAnalysisAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var analyzer = scope.ServiceProvider.GetRequiredService<ISprintHealthAnalyzer>();
                var alertsCreated = await analyzer.AnalyzeActiveSprintsAsync(cancellationToken);

                _logger.LogInformation(
                    "SprintHealthMonitorService: análise concluída, {AlertsCreated} novo(s) alerta(s) gerado(s)",
                    alertsCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar a análise de saúde das Sprints");
            }
        }
    }
}

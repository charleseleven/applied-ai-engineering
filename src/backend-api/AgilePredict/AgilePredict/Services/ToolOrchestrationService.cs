using System.Text.Json;
using AgilePredict.Models.DTOs;
using AgilePredict.Services.Interfaces;

namespace AgilePredict.Services
{
    /// <summary>
    /// Implementação da orquestração de tool calls (Task #138)
    /// </summary>
    public class ToolOrchestrationService : IToolOrchestrationService
    {
        private readonly ITaskAutomationService _taskAutomationService;
        private readonly ILogger<ToolOrchestrationService> _logger;

        public ToolOrchestrationService(
            ITaskAutomationService taskAutomationService, ILogger<ToolOrchestrationService> logger)
        {
            _taskAutomationService = taskAutomationService;
            _logger = logger;
        }

        public async Task<ToolCallOutcome> ProcessToolCallsAsync(
            IReadOnlyList<LlmToolCall> toolCalls, CancellationToken cancellationToken = default)
        {
            // Groq não suporta tool calls em paralelo, então na prática há sempre uma única
            // chamada aqui — mas o loop já cobre o caso de múltiplas, se isso mudar no futuro.
            var messages = new List<string>();
            var hasError = false;

            foreach (var toolCall in toolCalls)
            {
                switch (toolCall.Name)
                {
                    case ToolDefinitions.UpdateTaskStatusToolName:
                        var outcome = await HandleUpdateTaskStatusAsync(toolCall, cancellationToken);
                        messages.Add(outcome.Message);
                        hasError |= outcome.IsError;
                        break;

                    default:
                        _logger.LogWarning("Tool call para uma ação desconhecida: {ToolName}", toolCall.Name);
                        messages.Add($"A IA tentou executar uma ação desconhecida (\"{toolCall.Name}\") que não está implementada.");
                        hasError = true;
                        break;
                }
            }

            return new ToolCallOutcome { Message = string.Join("\n", messages), IsError = hasError };
        }

        private async Task<ToolCallOutcome> HandleUpdateTaskStatusAsync(
            LlmToolCall toolCall, CancellationToken cancellationToken)
        {
            int taskId;
            string status;
            try
            {
                var args = JsonSerializer.Deserialize<UpdateTaskStatusArgs>(
                    toolCall.ArgumentsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (args == null)
                {
                    return new ToolCallOutcome { Message = "Não foi possível interpretar os argumentos da ação.", IsError = true };
                }

                taskId = args.TaskId;
                status = args.Status;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Falha ao desserializar argumentos da tool call {ToolName}: {Arguments}",
                    toolCall.Name, toolCall.ArgumentsJson);
                return new ToolCallOutcome { Message = "A IA enviou argumentos inválidos para a ação de atualização de status.", IsError = true };
            }

            var result = await _taskAutomationService.UpdateTaskStatusAsync(taskId, status, cancellationToken);

            var prefix = result.Success ? "✅" : "❌";
            return new ToolCallOutcome { Message = $"{prefix} {result.Message}", IsError = !result.Success };
        }

        private class UpdateTaskStatusArgs
        {
            public int TaskId { get; set; }
            public string Status { get; set; } = string.Empty;
        }
    }
}

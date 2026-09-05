using AgilePredict.Models.DTOs;

namespace AgilePredict.Services.Interfaces
{
    /// <summary>
    /// Orquestra a execução das tool calls decididas pela LLM: identifica qual ação foi
    /// pedida, aciona o serviço de domínio correspondente e traduz o resultado real do
    /// backend numa mensagem de confirmação/erro (nunca na formulação livre da LLM).
    /// </summary>
    public interface IToolOrchestrationService
    {
        Task<ToolCallOutcome> ProcessToolCallsAsync(
            IReadOnlyList<LlmToolCall> toolCalls, CancellationToken cancellationToken = default);
    }
}

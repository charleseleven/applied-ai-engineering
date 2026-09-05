namespace AgilePredict.Models.DTOs
{
    /// <summary>
    /// Resultado do processamento de uma (ou mais) tool call(s) decidida(s) pela LLM,
    /// já traduzido para uma mensagem de confirmação/erro pronta para exibir ao usuário.
    /// </summary>
    public class ToolCallOutcome
    {
        public string Message { get; set; } = string.Empty;
        public bool IsError { get; set; }
    }
}

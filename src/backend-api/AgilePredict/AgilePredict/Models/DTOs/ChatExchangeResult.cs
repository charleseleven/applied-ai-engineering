using AgilePredict.Models;

namespace AgilePredict.Models.DTOs
{
    /// <summary>
    /// Resultado de um envio de mensagem: a mensagem do usuário e a resposta da IA já persistidas
    /// </summary>
    public class ChatExchangeResult
    {
        public int ConversationId { get; set; }
        public string ConversationTitle { get; set; } = string.Empty;
        public ChatMessage UserMessage { get; set; } = null!;
        public ChatMessage AssistantMessage { get; set; } = null!;
    }
}

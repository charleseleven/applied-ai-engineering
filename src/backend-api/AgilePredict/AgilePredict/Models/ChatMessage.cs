using System.Text.Json.Serialization;

namespace AgilePredict.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string Role { get; set; } = string.Empty; // "user" | "assistant"
        public string Content { get; set; } = string.Empty;
        public bool IsError { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relacionamento: Uma mensagem pertence a uma conversa
        // JsonIgnore evita serializar a referência de volta (conversation -> messages -> conversation -> ...)
        public int ConversationId { get; set; }
        [JsonIgnore]
        public ChatConversation? Conversation { get; set; }
    }
}

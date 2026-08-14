namespace AgilePredict.Models.DTOs
{
    /// <summary>
    /// Resumo de uma conversa para exibição na lista/sidebar (sem as mensagens)
    /// </summary>
    public class ChatConversationSummary
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

namespace AgilePredict.Models
{
    /// <summary>
    /// Log de auditoria simplificado para ações executadas autonomamente pela IA (function calling)
    /// </summary>
    public class AiAuditLog
    {
        public int Id { get; set; }
        public string EntityType { get; set; } = string.Empty; // ex: "ProjectTask"
        public int EntityId { get; set; }
        public string Action { get; set; } = string.Empty; // ex: "UpdateStatus"
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string Source { get; set; } = "AI";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

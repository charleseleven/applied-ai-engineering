using System.Text.Json.Serialization;

namespace AgilePredict.Models
{
    /// <summary>
    /// Alerta gerado pelo agente autônomo de monitoramento de Sprints (PBI #150) quando
    /// o ritmo de conclusão de Story Points está atrasado em relação ao tempo restante.
    /// </summary>
    public class SprintHealthAlert
    {
        public int Id { get; set; }
        public int SprintId { get; set; }

        [JsonIgnore]
        public Sprint? Sprint { get; set; }

        public string Severity { get; set; } = "Warning"; // Warning | Critical
        public string Message { get; set; } = string.Empty;
        public double CompletionPercent { get; set; }
        public double TimeElapsedPercent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Acknowledged { get; set; }
    }
}

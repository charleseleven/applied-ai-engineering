namespace AgilePredict.Models.DTOs
{
    /// <summary>
    /// Resultado de uma ação de automação executada sobre uma Task (via function calling da IA)
    /// </summary>
    public class TaskActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TaskId { get; set; }
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
    }
}

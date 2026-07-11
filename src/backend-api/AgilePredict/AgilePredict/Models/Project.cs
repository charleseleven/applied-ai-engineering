namespace AgilePredict.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Relacionamento: Um projeto pode ter várias sprints
        public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
    }
}

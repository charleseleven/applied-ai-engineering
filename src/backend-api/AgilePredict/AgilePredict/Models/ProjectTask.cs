namespace AgilePredict.Models
{
    public class ProjectTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "To Do";
        public int StoryPoints { get; set; }
        public string Priority { get; set; } = "Medium";
        public string? AssignedTo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Relacionamento: Uma task pertence a uma sprint
        public int SprintId { get; set; }
        public Sprint? Sprint { get; set; }
    }
}

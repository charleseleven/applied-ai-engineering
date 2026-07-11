namespace AgilePredict.Models
{
    public class Sprint
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Goal { get; set; } = string.Empty;
        public string Status { get; set; } = "Planning";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalStoryPoints { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Relacionamento: Uma sprint pertence a um projeto
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        // Relacionamento: Uma sprint pode ter várias tasks
        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    }
}

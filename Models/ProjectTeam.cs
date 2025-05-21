namespace ProjectManagementSystem.Models
{
    public class ProjectTeam
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; }

        public DateTime AssignedAt { get; set; }
    }
} 
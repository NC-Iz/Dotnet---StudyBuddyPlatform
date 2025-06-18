using System.ComponentModel.DataAnnotations;

namespace StudyBuddyPlatform.Models
{
    public class StudyTask
    {
        public int Id { get; set; }

        public int StudyPlanId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }

        [Required]
        public string Priority { get; set; } = "Medium"; // High, Medium, Low

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public StudyPlan? StudyPlan { get; set; }
    }
}
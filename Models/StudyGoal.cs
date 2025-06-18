using System.ComponentModel.DataAnnotations;

namespace StudyBuddyPlatform.Models
{
    public class StudyGoal
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime TargetDate { get; set; }

        [StringLength(100)]
        public string? Category { get; set; } = "Academic";

        public bool IsAchieved { get; set; } = false;

        public DateTime? AchievedDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public User? User { get; set; }
    }
}
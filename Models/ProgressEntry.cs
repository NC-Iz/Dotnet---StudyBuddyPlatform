using System.ComponentModel.DataAnnotations;

namespace StudyBuddyPlatform.Models
{
    public class ProgressEntry
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int? StudyPlanId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EntryDate { get; set; }

        [Range(0, 24)]
        public decimal StudyHours { get; set; } = 0;

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Range(1, 5)]
        public int? MoodRating { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public User? User { get; set; }
        public StudyPlan? StudyPlan { get; set; }
    }
}
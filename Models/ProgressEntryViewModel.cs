using System.ComponentModel.DataAnnotations;

namespace StudyBuddyPlatform.Models
{
    public class ProgressEntryViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime EntryDate { get; set; } = DateTime.Today;

        [Range(0, 24)]
        public decimal StudyHours { get; set; } = 0;

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Range(1, 5)]
        public int? MoodRating { get; set; }

        public int? StudyPlanId { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace StudyBuddyPlatform.Models
{
    public class GroupMessage
    {
        public int Id { get; set; }

        public int StudyGroupId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        public DateTime SentDate { get; set; } = DateTime.Now;

        // Navigation properties
        public StudyGroup? StudyGroup { get; set; }
        public User? User { get; set; }
    }
}
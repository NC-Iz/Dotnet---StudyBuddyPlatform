using System.ComponentModel.DataAnnotations;

namespace StudyBuddyPlatform.Models
{
    public class StudyResource
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Subject { get; set; }

        [Required]
        public string ResourceType { get; set; }

        public string? FileName { get; set; }

        public byte[]? FileContent { get; set; }

        public string? ContentType { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Add UserId for ownership
        public int UserId { get; set; }

        // Navigation property (optional)
        public User? User { get; set; }
    }
}
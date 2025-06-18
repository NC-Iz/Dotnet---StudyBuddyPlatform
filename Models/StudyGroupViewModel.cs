using System.ComponentModel.DataAnnotations;

namespace StudyBuddyPlatform.Models
{
    public class StudyGroupViewModel
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Subject { get; set; }

        [Range(2, 50)]
        public int MaxMembers { get; set; } = 10;

        public bool IsPublic { get; set; } = true;
    }
}
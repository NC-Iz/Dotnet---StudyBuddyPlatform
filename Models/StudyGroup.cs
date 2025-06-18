using System.ComponentModel.DataAnnotations;

namespace StudyBuddyPlatform.Models
{
    public class StudyGroup
    {
        public int Id { get; set; }

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

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int CreatedBy { get; set; }

        // Navigation properties
        public User? Creator { get; set; }
        public ICollection<StudyGroupMember>? Members { get; set; }
        public ICollection<GroupMessage>? Messages { get; set; }
    }
}
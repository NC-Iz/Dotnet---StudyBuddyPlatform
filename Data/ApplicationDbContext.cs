using Microsoft.EntityFrameworkCore;
using StudyBuddyPlatform.Models;

namespace StudyBuddyPlatform.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<StudyResource> StudyResources { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<StudyGroup> StudyGroups { get; set; }
        public DbSet<StudyGroupMember> StudyGroupMembers { get; set; }
        public DbSet<GroupMessage> GroupMessages { get; set; }
        public DbSet<StudyPlan> StudyPlans { get; set; }
        public DbSet<StudyTask> StudyTasks { get; set; }
        public DbSet<StudyGoal> StudyGoals { get; set; }
        public DbSet<ProgressEntry> ProgressEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships for Study Groups
            modelBuilder.Entity<StudyGroup>()
                .HasOne(sg => sg.Creator)
                .WithMany()
                .HasForeignKey(sg => sg.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudyGroupMember>()
                .HasIndex(sgm => new { sgm.StudyGroupId, sgm.UserId })
                .IsUnique();

            // Configure relationships for Study Plans
            modelBuilder.Entity<StudyPlan>()
                .HasOne(sp => sp.Creator)
                .WithMany()
                .HasForeignKey(sp => sp.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure unique constraint for progress entries (one per user per date)
            modelBuilder.Entity<ProgressEntry>()
                .HasIndex(pe => new { pe.UserId, pe.EntryDate })
                .IsUnique();

            // Configure decimal precision
            modelBuilder.Entity<ProgressEntry>()
                .Property(pe => pe.StudyHours)
                .HasPrecision(4, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}
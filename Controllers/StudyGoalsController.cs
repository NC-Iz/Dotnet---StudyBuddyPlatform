using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyBuddyPlatform.Data;
using StudyBuddyPlatform.Models;

namespace StudyBuddyPlatform.Controllers
{
    public class StudyGoalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudyGoalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StudyGoals
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to view your goals.";
                return RedirectToAction("Login", "Users");
            }

            var goals = await _context.StudyGoals
                .Where(g => g.UserId == userId)
                .OrderBy(g => g.IsAchieved)
                .ThenBy(g => g.TargetDate)
                .ToListAsync();

            return View(goals);
        }

        // POST: StudyGoals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string description, DateTime targetDate, string category)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["ErrorMessage"] = "Goal title is required.";
                return RedirectToAction("Index");
            }

            var goal = new StudyGoal
            {
                UserId = userId.Value,
                Title = title.Trim(),
                Description = description?.Trim(),
                TargetDate = targetDate,
                Category = category,
                IsAchieved = false,
                CreatedDate = DateTime.Now
            };

            _context.StudyGoals.Add(goal);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Goal created successfully!";
            return RedirectToAction("Index");
        }

        // POST: StudyGoals/ToggleAchieved/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAchieved(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var goal = await _context.StudyGoals
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (goal == null)
            {
                return NotFound();
            }

            goal.IsAchieved = !goal.IsAchieved;
            goal.AchievedDate = goal.IsAchieved ? DateTime.Now : null;

            _context.Update(goal);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = goal.IsAchieved ? "Congratulations! Goal achieved!" : "Goal marked as not achieved.";
            return RedirectToAction("Index");
        }

        // POST: StudyGoals/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var goal = await _context.StudyGoals
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (goal == null)
            {
                return NotFound();
            }

            _context.StudyGoals.Remove(goal);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Goal deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
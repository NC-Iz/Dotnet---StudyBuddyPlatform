using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyBuddyPlatform.Data;
using StudyBuddyPlatform.Models;

namespace StudyBuddyPlatform.Controllers
{
    public class StudyTasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudyTasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: StudyTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int studyPlanId, string title, string description, DateTime dueDate, string priority)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            // Verify user owns the study plan
            var studyPlan = await _context.StudyPlans
                .FirstOrDefaultAsync(sp => sp.Id == studyPlanId && sp.CreatedBy == userId);

            if (studyPlan == null)
            {
                TempData["ErrorMessage"] = "Study plan not found.";
                return RedirectToAction("Index", "StudyPlans");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["ErrorMessage"] = "Task title is required.";
                return RedirectToAction("Details", "StudyPlans", new { id = studyPlanId });
            }

            var task = new StudyTask
            {
                StudyPlanId = studyPlanId,
                Title = title.Trim(),
                Description = description?.Trim(),
                DueDate = dueDate,
                Priority = priority,
                IsCompleted = false,
                CreatedDate = DateTime.Now
            };

            _context.StudyTasks.Add(task);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Task added successfully!";
            return RedirectToAction("Details", "StudyPlans", new { id = studyPlanId });
        }

        // POST: StudyTasks/ToggleComplete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleComplete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var task = await _context.StudyTasks
                .Include(t => t.StudyPlan)
                .FirstOrDefaultAsync(t => t.Id == id && t.StudyPlan.CreatedBy == userId);

            if (task == null)
            {
                return NotFound();
            }

            task.IsCompleted = !task.IsCompleted;
            task.CompletedDate = task.IsCompleted ? DateTime.Now : null;

            _context.Update(task);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = task.IsCompleted ? "Task marked as completed!" : "Task marked as incomplete.";
            return RedirectToAction("Details", "StudyPlans", new { id = task.StudyPlanId });
        }

        // POST: StudyTasks/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var task = await _context.StudyTasks
                .Include(t => t.StudyPlan)
                .FirstOrDefaultAsync(t => t.Id == id && t.StudyPlan.CreatedBy == userId);

            if (task == null)
            {
                return NotFound();
            }

            var studyPlanId = task.StudyPlanId;
            _context.StudyTasks.Remove(task);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Task deleted successfully.";
            return RedirectToAction("Details", "StudyPlans", new { id = studyPlanId });
        }
    }
}
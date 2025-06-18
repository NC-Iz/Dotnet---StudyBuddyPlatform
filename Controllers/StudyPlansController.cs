using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyBuddyPlatform.Data;
using StudyBuddyPlatform.Models;

namespace StudyBuddyPlatform.Controllers
{
    public class StudyPlansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudyPlansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper method to auto-deactivate expired plans
        private async Task UpdateExpiredPlans()
        {
            var expiredPlans = await _context.StudyPlans
                .Where(sp => sp.IsActive && sp.EndDate < DateTime.Today)
                .ToListAsync();

            foreach (var plan in expiredPlans)
            {
                plan.IsActive = false;
            }

            if (expiredPlans.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        // GET: StudyPlans
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to view your study plans.";
                return RedirectToAction("Login", "Users");
            }

            // Auto-deactivate expired plans
            await UpdateExpiredPlans();

            var studyPlans = await _context.StudyPlans
                .Include(sp => sp.Tasks)
                .Where(sp => sp.CreatedBy == userId)
                .OrderByDescending(sp => sp.CreatedDate)
                .ToListAsync();

            return View(studyPlans);
        }

        // GET: StudyPlans/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }

        // POST: StudyPlans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudyPlanViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date");
            }

            if (ModelState.IsValid)
            {
                var studyPlan = new StudyPlan
                {
                    Title = model.Title,
                    Description = model.Description,
                    Subject = model.Subject,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    CreatedBy = userId.Value,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                _context.Add(studyPlan);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Study plan created successfully!";
                return RedirectToAction("Details", new { id = studyPlan.Id });
            }

            return View(model);
        }

        // GET: StudyPlans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            // Auto-deactivate expired plans
            await UpdateExpiredPlans();

            var studyPlan = await _context.StudyPlans
                .Include(sp => sp.Tasks.OrderBy(t => t.DueDate))
                .Include(sp => sp.ProgressEntries.OrderBy(pe => pe.EntryDate))
                .FirstOrDefaultAsync(sp => sp.Id == id && sp.CreatedBy == userId);

            if (studyPlan == null)
            {
                return NotFound();
            }

            return View(studyPlan);
        }

        // GET: StudyPlans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var studyPlan = await _context.StudyPlans
                .FirstOrDefaultAsync(sp => sp.Id == id && sp.CreatedBy == userId);

            if (studyPlan == null)
            {
                return NotFound();
            }

            var viewModel = new StudyPlanViewModel
            {
                Title = studyPlan.Title,
                Description = studyPlan.Description,
                Subject = studyPlan.Subject,
                StartDate = studyPlan.StartDate,
                EndDate = studyPlan.EndDate
            };

            return View(viewModel);
        }

        // POST: StudyPlans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudyPlanViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var studyPlan = await _context.StudyPlans
                        .FirstOrDefaultAsync(sp => sp.Id == id && sp.CreatedBy == userId);

                    if (studyPlan == null)
                    {
                        return NotFound();
                    }

                    studyPlan.Title = model.Title;
                    studyPlan.Description = model.Description;
                    studyPlan.Subject = model.Subject;
                    studyPlan.StartDate = model.StartDate;
                    studyPlan.EndDate = model.EndDate;

                    // Update active status based on new end date
                    if (studyPlan.EndDate < DateTime.Today)
                    {
                        studyPlan.IsActive = false;
                    }

                    _context.Update(studyPlan);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Study plan updated successfully!";
                    return RedirectToAction("Details", new { id = id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudyPlanExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(model);
        }

        // POST: StudyPlans/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var studyPlan = await _context.StudyPlans
                .Include(sp => sp.Tasks)           // Include related tasks
                .Include(sp => sp.ProgressEntries) // Include related progress entries
                .FirstOrDefaultAsync(sp => sp.Id == id && sp.CreatedBy == userId);

            if (studyPlan == null)
            {
                return NotFound();
            }

            try
            {
                // Delete related progress entries first
                if (studyPlan.ProgressEntries?.Any() == true)
                {
                    _context.ProgressEntries.RemoveRange(studyPlan.ProgressEntries);
                }

                // Delete related tasks (should cascade automatically, but being explicit)
                if (studyPlan.Tasks?.Any() == true)
                {
                    _context.StudyTasks.RemoveRange(studyPlan.Tasks);
                }

                // Delete the study plan
                _context.StudyPlans.Remove(studyPlan);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Study plan deleted successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting study plan. Please try again.";
                // Log the exception if you have logging configured
                return RedirectToAction("Details", new { id = id });
            }
        }

        // POST: StudyPlans/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var studyPlan = await _context.StudyPlans
                .FirstOrDefaultAsync(sp => sp.Id == id && sp.CreatedBy == userId);

            if (studyPlan == null)
            {
                return NotFound();
            }

            // Don't allow activation of expired plans
            if (!studyPlan.IsActive && studyPlan.EndDate < DateTime.Today)
            {
                TempData["ErrorMessage"] = "Cannot activate an expired study plan. Please extend the end date first.";
                return RedirectToAction("Details", new { id = id });
            }

            studyPlan.IsActive = !studyPlan.IsActive;
            _context.Update(studyPlan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = studyPlan.IsActive ? "Study plan activated." : "Study plan deactivated.";
            return RedirectToAction("Details", new { id = id });
        }

        // GET: StudyPlans/Progress
        public async Task<IActionResult> Progress()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var progressEntries = await _context.ProgressEntries
                .Include(pe => pe.StudyPlan)
                .Where(pe => pe.UserId == userId)
                .OrderByDescending(pe => pe.EntryDate)
                .Take(30) // Last 30 days
                .ToListAsync();

            var studyPlans = await _context.StudyPlans
                .Where(sp => sp.CreatedBy == userId && sp.IsActive)
                .ToListAsync();

            ViewBag.StudyPlans = studyPlans;
            return View(progressEntries);
        }

        // GET: StudyPlans/AddProgress
        public async Task<IActionResult> AddProgress()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var studyPlans = await _context.StudyPlans
                .Where(sp => sp.CreatedBy == userId && sp.IsActive)
                .ToListAsync();

            ViewBag.StudyPlans = studyPlans;
            return View();
        }

        // POST: StudyPlans/AddProgress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProgress(ProgressEntryViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            // Check if entry already exists for this date
            var existingEntry = await _context.ProgressEntries
                .FirstOrDefaultAsync(pe => pe.UserId == userId && pe.EntryDate.Date == model.EntryDate.Date);

            if (existingEntry != null)
            {
                ModelState.AddModelError("EntryDate", "You already have a progress entry for this date. Please edit the existing entry or choose a different date.");
            }

            if (ModelState.IsValid)
            {
                var progressEntry = new ProgressEntry
                {
                    UserId = userId.Value,
                    EntryDate = model.EntryDate,
                    StudyHours = model.StudyHours,
                    Notes = model.Notes,
                    MoodRating = model.MoodRating,
                    StudyPlanId = model.StudyPlanId,
                    CreatedDate = DateTime.Now
                };

                _context.ProgressEntries.Add(progressEntry);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Progress entry added successfully!";
                return RedirectToAction("Progress");
            }

            var studyPlans = await _context.StudyPlans
                .Where(sp => sp.CreatedBy == userId && sp.IsActive)
                .ToListAsync();

            ViewBag.StudyPlans = studyPlans;
            return View(model);
        }

        private bool StudyPlanExists(int id)
        {
            return _context.StudyPlans.Any(e => e.Id == id);
        }
    }
}
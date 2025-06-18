using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyBuddyPlatform.Data;
using StudyBuddyPlatform.Models;

namespace StudyBuddyPlatform.Controllers
{
    public class StudyResourcesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudyResourcesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StudyResources - Show only current user's resources
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to view your study resources.";
                return RedirectToAction("Login", "Users");
            }

            var userResources = await _context.StudyResources
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return View(userResources);
        }

        // GET: StudyResources/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }

        // POST: StudyResources/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Subject,ResourceType")] StudyResource studyResource, IFormFile file)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            // Remove validation errors for file-related properties
            ModelState.Remove("FileName");
            ModelState.Remove("FileContent");
            ModelState.Remove("ContentType");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                // Set the current user as owner
                studyResource.UserId = userId.Value;

                if (file != null && file.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        studyResource.FileName = file.FileName;
                        studyResource.FileContent = stream.ToArray();
                        studyResource.ContentType = file.ContentType;
                    }
                }

                _context.Add(studyResource);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Study resource created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(studyResource);
        }

        // GET: StudyResources/Details/5
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

            var studyResource = await _context.StudyResources
                .Where(r => r.Id == id && r.UserId == userId) // Only show user's own resources
                .FirstOrDefaultAsync();

            if (studyResource == null)
            {
                return NotFound();
            }

            return View(studyResource);
        }

        // GET: StudyResources/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to edit resources.";
                return RedirectToAction("Login", "Users");
            }

            var studyResource = await _context.StudyResources
                .Where(r => r.Id == id && r.UserId == userId)
                .FirstOrDefaultAsync();

            if (studyResource == null)
            {
                TempData["ErrorMessage"] = "Resource not found or you don't have permission to edit it.";
                return RedirectToAction("Index");
            }

            return View(studyResource);
        }

        // POST: StudyResources/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile file)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to edit resources.";
                return RedirectToAction("Login", "Users");
            }

            var existingResource = await _context.StudyResources
                .Where(r => r.Id == id && r.UserId == userId)
                .FirstOrDefaultAsync();

            if (existingResource == null)
            {
                TempData["ErrorMessage"] = "Resource not found or you don't have permission to edit it.";
                return RedirectToAction("Index");
            }

            // Get form values manually
            var title = Request.Form["Title"];
            var description = Request.Form["Description"];
            var subject = Request.Form["Subject"];
            var resourceType = Request.Form["ResourceType"];

            // Validate required fields
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(resourceType))
            {
                TempData["ErrorMessage"] = "Title, Subject, and Resource Type are required.";
                return View(existingResource);
            }

            try
            {
                // Update the basic properties
                existingResource.Title = title;
                existingResource.Description = description;
                existingResource.Subject = subject;
                existingResource.ResourceType = resourceType;

                // Only update file if a new file is uploaded
                if (file != null && file.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        existingResource.FileName = file.FileName;
                        existingResource.FileContent = stream.ToArray();
                        existingResource.ContentType = file.ContentType;
                    }
                }

                _context.Update(existingResource);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Study resource updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the resource.";
                Console.WriteLine($"Error: {ex.Message}");
                return View(existingResource);
            }
        }

        // GET: StudyResources/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

            var studyResource = await _context.StudyResources
                .Where(r => r.Id == id && r.UserId == userId) // Only allow deleting own resources
                .FirstOrDefaultAsync();

            if (studyResource == null)
            {
                return NotFound();
            }

            return View(studyResource);
        }

        // POST: StudyResources/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var studyResource = await _context.StudyResources
                .Where(r => r.Id == id && r.UserId == userId) // Only allow deleting own resources
                .FirstOrDefaultAsync();

            if (studyResource != null)
            {
                _context.StudyResources.Remove(studyResource);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Study resource deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Download file - only allow downloading own files
        public async Task<IActionResult> Download(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var file = await _context.StudyResources
                .Where(r => r.Id == id && r.UserId == userId)
                .FirstOrDefaultAsync();

            if (file == null)
                return NotFound();

            return File(file.FileContent, file.ContentType, file.FileName);
        }

        private bool StudyResourceExists(int id)
        {
            return _context.StudyResources.Any(e => e.Id == id);
        }
    }
}
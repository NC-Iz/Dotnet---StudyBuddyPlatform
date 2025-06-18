using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyBuddyPlatform.Data;
using StudyBuddyPlatform.Models;

namespace StudyBuddyPlatform.Controllers
{
    public class StudyGroupsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudyGroupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StudyGroups - Browse public groups
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to view study groups.";
                return RedirectToAction("Login", "Users");
            }

            var publicGroups = await _context.StudyGroups
                .Include(sg => sg.Creator)
                .Include(sg => sg.Members)
                .Where(sg => sg.IsPublic == true)
                .OrderByDescending(sg => sg.CreatedDate)
                .ToListAsync();

            return View(publicGroups);
        }

        // GET: StudyGroups/MyGroups - Show groups user is member of
        public async Task<IActionResult> MyGroups()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to view your groups.";
                return RedirectToAction("Login", "Users");
            }

            var myGroups = await _context.StudyGroupMembers
                .Include(sgm => sgm.StudyGroup)
                .ThenInclude(sg => sg.Creator)
                .Include(sgm => sgm.StudyGroup.Members)
                .Where(sgm => sgm.UserId == userId)
                .Select(sgm => sgm.StudyGroup)
                .OrderByDescending(sg => sg.CreatedDate)
                .ToListAsync();

            return View(myGroups);
        }

        // GET: StudyGroups/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }

        // POST: StudyGroups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudyGroupViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            if (ModelState.IsValid)
            {
                var studyGroup = new StudyGroup
                {
                    Name = model.Name,
                    Description = model.Description,
                    Subject = model.Subject,
                    MaxMembers = model.MaxMembers,
                    IsPublic = model.IsPublic,
                    CreatedBy = userId.Value,
                    CreatedDate = DateTime.Now
                };

                _context.Add(studyGroup);
                await _context.SaveChangesAsync();

                // Add creator as first member with Creator role
                var creatorMember = new StudyGroupMember
                {
                    StudyGroupId = studyGroup.Id,
                    UserId = userId.Value,
                    Role = "Creator",
                    JoinedDate = DateTime.Now
                };

                _context.StudyGroupMembers.Add(creatorMember);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Study group created successfully!";
                return RedirectToAction("Details", new { id = studyGroup.Id });
            }

            return View(model);
        }

        // GET: StudyGroups/Details/5
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

            var studyGroup = await _context.StudyGroups
                .Include(sg => sg.Creator)
                .Include(sg => sg.Members)
                .ThenInclude(m => m.User)
                .Include(sg => sg.Messages)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(sg => sg.Id == id);

            if (studyGroup == null)
            {
                return NotFound();
            }

            // Check if user is a member
            var isMember = studyGroup.Members?.Any(m => m.UserId == userId) ?? false;
            ViewBag.IsMember = isMember;
            ViewBag.CurrentUserId = userId;

            // Get user's role in the group
            var userRole = studyGroup.Members?.FirstOrDefault(m => m.UserId == userId)?.Role ?? "";
            ViewBag.UserRole = userRole;

            return View(studyGroup);
        }

        // GET: StudyGroups/Edit/5
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

            var studyGroup = await _context.StudyGroups
                .Include(sg => sg.Members)
                .FirstOrDefaultAsync(sg => sg.Id == id);

            if (studyGroup == null)
            {
                return NotFound();
            }

            // Check if user is creator or admin
            var userMember = studyGroup.Members?.FirstOrDefault(m => m.UserId == userId);
            if (userMember == null || (userMember.Role != "Creator" && userMember.Role != "Admin"))
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this group.";
                return RedirectToAction("Details", new { id = id });
            }

            var viewModel = new StudyGroupViewModel
            {
                Name = studyGroup.Name,
                Description = studyGroup.Description,
                Subject = studyGroup.Subject,
                MaxMembers = studyGroup.MaxMembers,
                IsPublic = studyGroup.IsPublic
            };

            return View(viewModel);
        }

        // POST: StudyGroups/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudyGroupViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var studyGroup = await _context.StudyGroups
                        .Include(sg => sg.Members)
                        .FirstOrDefaultAsync(sg => sg.Id == id);

                    if (studyGroup == null)
                    {
                        return NotFound();
                    }

                    // Check permissions
                    var userMember = studyGroup.Members?.FirstOrDefault(m => m.UserId == userId);
                    if (userMember == null || (userMember.Role != "Creator" && userMember.Role != "Admin"))
                    {
                        TempData["ErrorMessage"] = "You don't have permission to edit this group.";
                        return RedirectToAction("Details", new { id = id });
                    }

                    studyGroup.Name = model.Name;
                    studyGroup.Description = model.Description;
                    studyGroup.Subject = model.Subject;
                    studyGroup.MaxMembers = model.MaxMembers;
                    studyGroup.IsPublic = model.IsPublic;

                    _context.Update(studyGroup);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Study group updated successfully!";
                    return RedirectToAction("Details", new { id = id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudyGroupExists(id))
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

        // POST: StudyGroups/Join/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var studyGroup = await _context.StudyGroups
                .Include(sg => sg.Members)
                .FirstOrDefaultAsync(sg => sg.Id == id);

            if (studyGroup == null)
            {
                return NotFound();
            }

            // Check if already a member
            if (studyGroup.Members?.Any(m => m.UserId == userId) == true)
            {
                TempData["ErrorMessage"] = "You are already a member of this group.";
                return RedirectToAction("Details", new { id = id });
            }

            // Check if group is full
            if (studyGroup.Members?.Count >= studyGroup.MaxMembers)
            {
                TempData["ErrorMessage"] = "This group is full.";
                return RedirectToAction("Details", new { id = id });
            }

            var newMember = new StudyGroupMember
            {
                StudyGroupId = id,
                UserId = userId.Value,
                Role = "Member",
                JoinedDate = DateTime.Now
            };

            _context.StudyGroupMembers.Add(newMember);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Successfully joined the study group!";
            return RedirectToAction("Details", new { id = id });
        }

        // POST: StudyGroups/Leave/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var membership = await _context.StudyGroupMembers
                .FirstOrDefaultAsync(sgm => sgm.StudyGroupId == id && sgm.UserId == userId);

            if (membership == null)
            {
                TempData["ErrorMessage"] = "You are not a member of this group.";
                return RedirectToAction("Details", new { id = id });
            }

            // If creator is leaving, check if there are other members
            if (membership.Role == "Creator")
            {
                var otherMembers = await _context.StudyGroupMembers
                    .Where(sgm => sgm.StudyGroupId == id && sgm.UserId != userId)
                    .ToListAsync();

                if (otherMembers.Any())
                {
                    TempData["ErrorMessage"] = "As the creator, you cannot leave the group while there are other members. Please transfer ownership or remove all members first.";
                    return RedirectToAction("Details", new { id = id });
                }
            }

            _context.StudyGroupMembers.Remove(membership);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Successfully left the study group.";
            return RedirectToAction("Index");
        }

        // POST: StudyGroups/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var studyGroup = await _context.StudyGroups
                .Include(sg => sg.Members)
                .FirstOrDefaultAsync(sg => sg.Id == id);

            if (studyGroup == null)
            {
                return NotFound();
            }

            // Check if user is the creator
            var userMember = studyGroup.Members?.FirstOrDefault(m => m.UserId == userId);
            if (userMember?.Role != "Creator")
            {
                TempData["ErrorMessage"] = "Only the group creator can delete the group.";
                return RedirectToAction("Details", new { id = id });
            }

            _context.StudyGroups.Remove(studyGroup);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Study group deleted successfully.";
            return RedirectToAction("MyGroups");
        }

        // POST: StudyGroups/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int groupId, string message)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["ErrorMessage"] = "Message cannot be empty.";
                return RedirectToAction("Details", new { id = groupId });
            }

            // Check if user is a member
            var isMember = await _context.StudyGroupMembers
                .AnyAsync(sgm => sgm.StudyGroupId == groupId && sgm.UserId == userId);

            if (!isMember)
            {
                TempData["ErrorMessage"] = "You must be a member to send messages.";
                return RedirectToAction("Details", new { id = groupId });
            }

            var groupMessage = new GroupMessage
            {
                StudyGroupId = groupId,
                UserId = userId.Value,
                Message = message.Trim(),
                SentDate = DateTime.Now
            };

            _context.GroupMessages.Add(groupMessage);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = groupId });
        }

        private bool StudyGroupExists(int id)
        {
            return _context.StudyGroups.Any(e => e.Id == id);
        }
    }
}
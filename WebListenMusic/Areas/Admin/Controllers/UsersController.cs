using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebListenMusic.Models;
using WebListenMusic.Models.ViewModels;

namespace WebListenMusic.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(string? search, bool? isPremium, int page = 1)
        {
            const int pageSize = 15;

            var query = _userManager.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Email!.Contains(search) || 
                                        (u.DisplayName != null && u.DisplayName.Contains(search)));
            }

            if (isPremium.HasValue)
            {
                query = query.Where(u => u.IsPremium == isPremium.Value);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new AdminUserViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    DisplayName = u.DisplayName,
                    AvatarUrl = u.AvatarUrl,
                    IsPremium = u.IsPremium,
                    IsActive = u.IsActive,
                    EmailConfirmed = u.EmailConfirmed,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                })
                .ToListAsync();

            // Get roles for each user
            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.Id);
                if (appUser != null)
                {
                    user.Roles = (await _userManager.GetRolesAsync(appUser)).ToList();
                }
            }

            ViewBag.Search = search;
            ViewBag.IsPremium = isPremium;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(users);
        }

        // GET: Admin/Users/Details/id
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            
            // Get user statistics
            var playlistCount = await _context.Playlists.CountAsync(p => p.UserId == id);

            ViewBag.Roles = roles;
            ViewBag.PlaylistCount = playlistCount;

            return View(user);
        }

        // POST: Admin/Users/ToggleStatus
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            return Json(new { 
                success = true, 
                isActive = user.IsActive,
                message = user.IsActive ? "Account activated" : "Account locked" 
            });
        }

        // POST: Admin/Users/TogglePremium
        [HttpPost]
        public async Task<IActionResult> TogglePremium(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            user.IsPremium = !user.IsPremium;
            
            await _userManager.UpdateAsync(user);

            return Json(new { 
                success = true, 
                isPremium = user.IsPremium,
                message = user.IsPremium ? "Premium upgraded" : "Premium cancelled" 
            });
        }

        // POST: Admin/Users/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if user is admin
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Error"] = "Cannot delete Admin account";
                return RedirectToAction(nameof(Index));
            }

            // Delete related data
            var playlists = await _context.Playlists.Where(p => p.UserId == id).ToListAsync();
            _context.Playlists.RemoveRange(playlists);

            var reports = await _context.Reports.Where(r => r.UserId == id).ToListAsync();
            _context.Reports.RemoveRange(reports);

            await _context.SaveChangesAsync();

            // Delete user
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "User deleted successfully";
            }
            else
            {
                TempData["Error"] = "Cannot delete user";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

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
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Reports
        public async Task<IActionResult> Index(ReportStatus? status, ReportType? type, int page = 1)
        {
            const int pageSize = 15;

            var query = _context.Reports
                .Include(r => r.User)
                .Include(r => r.RelatedSong)
                .Include(r => r.RelatedAlbum)
                .AsQueryable();

            // Apply filters
            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            if (type.HasValue)
            {
                query = query.Where(r => r.Type == type.Value);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var reports = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Status = status;
            ViewBag.Type = type;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(reports);
        }

        // GET: Admin/Reports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports
                .Include(r => r.User)
                .Include(r => r.RelatedSong)
                    .ThenInclude(s => s!.Artist)
                .Include(r => r.RelatedAlbum)
                    .ThenInclude(a => a!.Artist)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: Admin/Reports/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ReportStatus status, string? adminNote)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            report.Status = status;
            report.AdminNote = adminNote;
            report.UpdatedAt = DateTime.UtcNow;

            if (status == ReportStatus.Resolved || status == ReportStatus.Dismissed)
            {
                report.ResolvedAt = DateTime.UtcNow;
                report.ResolvedByUserId = currentUser?.Id;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Report status updated successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/Reports/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Report deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}

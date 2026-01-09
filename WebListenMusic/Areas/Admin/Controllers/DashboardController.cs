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
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            
            var viewModel = new AdminDashboardViewModel
            {
                TotalSongs = await _context.Songs.CountAsync(),
                TotalAlbums = await _context.Albums.CountAsync(),
                TotalArtists = await _context.Artists.CountAsync(),
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalPlaylists = await _context.Playlists.CountAsync(),
                PendingReports = await _context.Reports.CountAsync(r => r.Status == ReportStatus.Pending),
                NewUsersThisMonth = await _userManager.Users.CountAsync(u => u.CreatedAt >= startOfMonth),
                NewSongsThisMonth = await _context.Songs.CountAsync(s => s.CreatedAt >= startOfMonth),
                
                RecentSongs = await _context.Songs
                    .Include(s => s.Artist)
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                    
                RecentUsers = await _userManager.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                    
                RecentReports = await _context.Reports
                    .Include(r => r.User)
                    .Where(r => r.Status == ReportStatus.Pending)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                    
                TopSongs = await _context.Songs
                    .Include(s => s.Artist)
                    .OrderByDescending(s => s.PlayCount)
                    .Take(10)
                    .ToListAsync()
            };

            // Get chart data for last 6 months
            var months = new List<string>();
            var songUploads = new List<int>();
            var userRegistrations = new List<int>();

            for (int i = 5; i >= 0; i--)
            {
                var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1);
                
                months.Add(monthStart.ToString("MMM yyyy"));
                songUploads.Add(await _context.Songs.CountAsync(s => s.CreatedAt >= monthStart && s.CreatedAt < monthEnd));
                userRegistrations.Add(await _userManager.Users.CountAsync(u => u.CreatedAt >= monthStart && u.CreatedAt < monthEnd));
            }

            viewModel.MonthLabels = months;
            viewModel.SongUploadsPerMonth = songUploads;
            viewModel.UserRegistrationsPerMonth = userRegistrations;

            return View(viewModel);
        }
    }
}

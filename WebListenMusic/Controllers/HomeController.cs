using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebListenMusic.Models;
using WebListenMusic.Models.ViewModels;

namespace WebListenMusic.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel
            {
                // Featured Songs (Nổi bật)
                FeaturedSongs = await _context.Songs
                    .Include(s => s.Artist)
                    .Where(s => s.IsPublished && s.IsFeatured)
                    .OrderByDescending(s => s.PlayCount)
                    .Take(10)
                    .ToListAsync(),

                // Trending Songs (Xu hướng - nhiều lượt nghe nhất)
                TrendingSongs = await _context.Songs
                    .Include(s => s.Artist)
                    .Where(s => s.IsPublished)
                    .OrderByDescending(s => s.PlayCount)
                    .Take(12)
                    .ToListAsync(),

                // New Releases (Mới phát hành)
                NewSongs = await _context.Songs
                    .Include(s => s.Artist)
                    .Where(s => s.IsPublished)
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(12)
                    .ToListAsync(),

                // Popular Albums
                PopularAlbums = await _context.Albums
                    .Include(a => a.Artist)
                    .Where(a => a.IsPublished)
                    .OrderByDescending(a => a.Songs.Sum(s => s.PlayCount))
                    .Take(6)
                    .ToListAsync(),

                // Top Artists
                TopArtists = await _context.Artists
                    .AsQueryable()
                    .OrderByDescending(a => a.Songs.Sum(s => s.PlayCount))
                    .Take(6)
                    .ToListAsync(),

                // Genres
                Genres = await _context.Genres
                    .AsQueryable()
                    .OrderBy(g => g.Name)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

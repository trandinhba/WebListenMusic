using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebListenMusic.Models;
using WebListenMusic.Models.ViewModels;

namespace WebListenMusic.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Search
        public async Task<IActionResult> Index(string? q, string type = "all")
        {
            if (string.IsNullOrEmpty(q))
            {
                return View(new SearchViewModel { Query = q, Type = type });
            }

            var viewModel = new SearchViewModel
            {
                Query = q,
                Type = type
            };

            // Search Songs
            if (type == "all" || type == "songs")
            {
                viewModel.Songs = await _context.Songs
                    .Include(s => s.Artist)
                    .Where(s => s.IsPublished && 
                               (s.Title.Contains(q) || (s.Artist != null && s.Artist.Name.Contains(q))))
                    .OrderByDescending(s => s.PlayCount)
                    .Take(type == "all" ? 6 : 20)
                    .ToListAsync();
            }

            // Search Albums
            if (type == "all" || type == "albums")
            {
                viewModel.Albums = await _context.Albums
                    .Include(a => a.Artist)
                    .Where(a => a.IsPublished && 
                               (a.Title.Contains(q) || (a.Artist != null && a.Artist.Name.Contains(q))))
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(type == "all" ? 4 : 20)
                    .ToListAsync();
            }

            // Search Artists
            if (type == "all" || type == "artists")
            {
                viewModel.Artists = await _context.Artists
                    .Include(a => a.Songs)
                    .Where(a => a.Name.Contains(q))
                    .OrderByDescending(a => a.Songs.Sum(s => s.PlayCount))
                    .Take(type == "all" ? 6 : 20)
                    .ToListAsync();
            }

            // Search Playlists
            if (type == "all" || type == "playlists")
            {
                viewModel.Playlists = await _context.Playlists
                    .Include(p => p.User)
                    .Include(p => p.PlaylistSongs)
                    .Where(p => p.IsPublic && p.Name.Contains(q))
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(type == "all" ? 4 : 20)
                    .ToListAsync();
            }

            return View(viewModel);
        }

        // GET: Search/Quick (for autocomplete)
        [HttpGet]
        public async Task<IActionResult> Quick(string q)
        {
            if (string.IsNullOrEmpty(q) || q.Length < 2)
            {
                return Json(new { songs = new List<object>(), artists = new List<object>() });
            }

            var songs = await _context.Songs
                .Include(s => s.Artist)
                .Where(s => s.IsPublished && s.Title.Contains(q))
                .Take(5)
                .Select(s => new { s.Id, s.Title, artist = s.Artist != null ? s.Artist.Name : "Unknown", s.CoverImageUrl })
                .ToListAsync();

            var artists = await _context.Artists
                .Where(a => a.Name.Contains(q))
                .Take(3)
                .Select(a => new { a.Id, a.Name, a.ImageUrl })
                .ToListAsync();

            return Json(new { songs, artists });
        }
    }
}

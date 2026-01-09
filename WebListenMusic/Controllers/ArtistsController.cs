using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebListenMusic.Models;
using WebListenMusic.Models.ViewModels;

namespace WebListenMusic.Controllers
{
    public class ArtistsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 24;

        public ArtistsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Artists
        public async Task<IActionResult> Index(string? search, string sort = "popular", int page = 1)
        {
            var query = _context.Artists
                .Include(a => a.Songs)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => a.Name.Contains(search));
            }

            // Sorting
            query = sort switch
            {
                "newest" => query.OrderByDescending(a => a.CreatedAt),
                "name" => query.OrderBy(a => a.Name),
                "songs" => query.OrderByDescending(a => a.Songs.Count),
                _ => query.OrderByDescending(a => a.Songs.Sum(s => s.PlayCount)) // popular
            };

            var totalItems = await query.CountAsync();
            var artists = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var viewModel = new ArtistsViewModel
            {
                Artists = artists,
                Search = search,
                Sort = sort,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize),
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        // GET: Artists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var artist = await _context.Artists
                .Include(a => a.Songs.Where(s => s.IsPublished).OrderByDescending(s => s.PlayCount))
                .Include(a => a.Albums.Where(al => al.IsPublished).OrderByDescending(al => al.CreatedAt))
                .FirstOrDefaultAsync(a => a.Id == id);

            if (artist == null) return NotFound();

            // Related artists (same genre or random)
            var relatedArtists = await _context.Artists
                .Include(a => a.Songs)
                .Where(a => a.Id != artist.Id)
                .OrderByDescending(a => a.Songs.Sum(s => s.PlayCount))
                .Take(6)
                .ToListAsync();

            ViewBag.RelatedArtists = relatedArtists;

            return View(artist);
        }
    }
}

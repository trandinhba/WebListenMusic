using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebListenMusic.Models;
using WebListenMusic.Models.ViewModels;

namespace WebListenMusic.Controllers
{
    public class AlbumsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 20;

        public AlbumsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Albums
        public async Task<IActionResult> Index(string? search, int? artist, string sort = "newest", int page = 1)
        {
            var query = _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Songs)
                .Where(a => a.IsPublished);

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => a.Title.Contains(search) ||
                                        (a.Artist != null && a.Artist.Name.Contains(search)));
            }

            // Filter by Artist
            if (artist.HasValue)
            {
                query = query.Where(a => a.ArtistId == artist.Value);
            }

            // Sorting
            query = sort switch
            {
                "popular" => query.OrderByDescending(a => a.Songs.Sum(s => s.PlayCount)),
                "oldest" => query.OrderBy(a => a.CreatedAt),
                "title" => query.OrderBy(a => a.Title),
                _ => query.OrderByDescending(a => a.CreatedAt) // newest
            };

            var totalItems = await query.CountAsync();
            var albums = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var viewModel = new AlbumsViewModel
            {
                Albums = albums,
                Search = search,
                ArtistId = artist,
                Sort = sort,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize),
                TotalItems = totalItems,
                Artists = await _context.Artists.Take(20).ToListAsync()
            };

            if (artist.HasValue)
            {
                var selectedArtist = await _context.Artists.FindAsync(artist.Value);
                viewModel.SelectedArtistName = selectedArtist?.Name;
            }

            return View(viewModel);
        }

        // GET: Albums/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var album = await _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Songs.Where(s => s.IsPublished).OrderBy(s => s.Title))
                    .ThenInclude(s => s.Artist)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsPublished);

            if (album == null) return NotFound();

            // Other albums by same artist
            var otherAlbums = await _context.Albums
                .Include(a => a.Artist)
                .Where(a => a.IsPublished && a.Id != album.Id && a.ArtistId == album.ArtistId)
                .Take(4)
                .ToListAsync();

            ViewBag.OtherAlbums = otherAlbums;

            return View(album);
        }

        // GET: Albums/GetAlbumSongs/5 (for player)
        [HttpGet]
        public async Task<IActionResult> GetAlbumSongs(int id)
        {
            var album = await _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Songs.Where(s => s.IsPublished).OrderBy(s => s.Title))
                    .ThenInclude(s => s.Artist)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsPublished);

            if (album == null) return NotFound();

            var songs = album.Songs.Select(s => new
            {
                id = s.Id,
                title = s.Title,
                artist = s.Artist?.Name ?? album.Artist?.Name ?? "Unknown",
                artistId = s.ArtistId ?? album.ArtistId,
                audioUrl = s.AudioUrl,
                coverUrl = s.CoverImageUrl ?? album.CoverImageUrl ?? "/uploads/covers/default-song.jpg",
                duration = s.Duration
            });

            return Json(songs);
        }
    }
}

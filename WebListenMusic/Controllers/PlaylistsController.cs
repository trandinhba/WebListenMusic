using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebListenMusic.Models;
using WebListenMusic.Models.ViewModels;

namespace WebListenMusic.Controllers
{
    public class PlaylistsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const int PageSize = 20;

        public PlaylistsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Playlists
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var currentUserId = _userManager.GetUserId(User);

            var query = _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                .Where(p => p.IsPublic || p.UserId == currentUserId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            query = query.OrderByDescending(p => p.CreatedAt);

            var totalItems = await query.CountAsync();
            var playlists = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var viewModel = new PlaylistsViewModel
            {
                Playlists = playlists,
                Search = search,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize),
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        // GET: Playlists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            var playlist = await _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistSongs.OrderBy(ps => ps.Order))
                    .ThenInclude(ps => ps.Song)
                        .ThenInclude(s => s.Artist)
                .FirstOrDefaultAsync(p => p.Id == id && (p.IsPublic || p.UserId == currentUserId));

            if (playlist == null) return NotFound();

            ViewBag.IsOwner = playlist.UserId == currentUserId;

            return View(playlist);
        }

        // GET: Playlists/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Playlists/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Playlist playlist)
        {
            // Remove validation for UserId since it's set server-side
            ModelState.Remove("UserId");
            
            if (ModelState.IsValid)
            {
                playlist.UserId = _userManager.GetUserId(User)!;
                playlist.CreatedAt = DateTime.Now;
                
                _context.Add(playlist);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Playlist created successfully!";
                return RedirectToAction(nameof(Details), new { id = playlist.Id });
            }
            return View(playlist);
        }

        // GET: Playlists/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUserId);

            if (playlist == null) return NotFound();

            return View(playlist);
        }

        // POST: Playlists/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Playlist playlist)
        {
            if (id != playlist.Id) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var existingPlaylist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUserId);

            if (existingPlaylist == null) return NotFound();

            if (ModelState.IsValid)
            {
                existingPlaylist.Name = playlist.Name;
                existingPlaylist.Description = playlist.Description;
                existingPlaylist.IsPublic = playlist.IsPublic;
                existingPlaylist.CoverImageUrl = playlist.CoverImageUrl;
                existingPlaylist.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(playlist);
        }

        // POST: Playlists/Delete/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUserId);

            if (playlist == null) return NotFound();

            _context.PlaylistSongs.RemoveRange(playlist.PlaylistSongs);
            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Playlists/AddSong
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddSong(int playlistId, int songId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == currentUserId);

            if (playlist == null)
                return Json(new { success = false, message = "Playlist not found" });

            if (playlist.PlaylistSongs.Any(ps => ps.SongId == songId))
                return Json(new { success = false, message = "Song already in playlist" });

            var maxOrder = playlist.PlaylistSongs.Any() ? playlist.PlaylistSongs.Max(ps => ps.Order) : 0;

            var playlistSong = new PlaylistSong
            {
                PlaylistId = playlistId,
                SongId = songId,
                Order = maxOrder + 1,
                AddedAt = DateTime.Now
            };

            _context.PlaylistSongs.Add(playlistSong);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Added to playlist" });
        }

        // POST: Playlists/RemoveSong
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveSong(int playlistId, int songId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == currentUserId);

            if (playlist == null)
                return Json(new { success = false, message = "Playlist not found" });

            var playlistSong = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

            if (playlistSong == null)
                return Json(new { success = false, message = "Song not in playlist" });

            _context.PlaylistSongs.Remove(playlistSong);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Removed from playlist" });
        }

        // GET: Playlists/GetUserPlaylists (for modal)
        [HttpGet]
        public async Task<IActionResult> GetUserPlaylists()
        {
            var currentUserId = _userManager.GetUserId(User);
            
            // Return empty array if not logged in
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Json(new { success = false, message = "Please login", playlists = new List<object>() });
            }
            
            var playlists = await _context.Playlists
                .Where(p => p.UserId == currentUserId)
                .Select(p => new { id = p.Id, name = p.Name, songCount = p.PlaylistSongs.Count })
                .ToListAsync();

            return Json(new { success = true, playlists = playlists });
        }

        // GET: Playlists/GetPlaylistSongs/5 (for player)
        [HttpGet]
        public async Task<IActionResult> GetPlaylistSongs(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs.OrderBy(ps => ps.Order))
                    .ThenInclude(ps => ps.Song)
                        .ThenInclude(s => s.Artist)
                .FirstOrDefaultAsync(p => p.Id == id && (p.IsPublic || p.UserId == currentUserId));

            if (playlist == null)
                return Json(new List<object>());

            var songs = playlist.PlaylistSongs.Select(ps => new
            {
                id = ps.Song.Id,
                title = ps.Song.Title,
                artistName = ps.Song.Artist?.Name ?? "Unknown Artist",
                audioUrl = ps.Song.AudioUrl,
                coverImageUrl = ps.Song.CoverImageUrl ?? "/uploads/covers/default-song.jpg",
                duration = ps.Song.Duration
            }).ToList();

            return Json(songs);
        }

        // POST: Playlists/CreateQuick (AJAX - create playlist from modal)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateQuick([FromBody] CreatePlaylistRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
                return Json(new { success = false, message = "Playlist name is required" });

            var currentUserId = _userManager.GetUserId(User);
            
            var playlist = new Playlist
            {
                Name = request.Name.Trim(),
                UserId = currentUserId!,
                IsPublic = false,
                CreatedAt = DateTime.Now
            };

            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Playlist created", playlistId = playlist.Id });
        }

        public class CreatePlaylistRequest
        {
            public string? Name { get; set; }
        }
    }
}

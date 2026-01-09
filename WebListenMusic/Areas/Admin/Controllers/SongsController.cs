using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebListenMusic.Helpers;
using WebListenMusic.Models;
using WebListenMusic.Models.ViewModels;

namespace WebListenMusic.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SongsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SongsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Admin/Songs
        public async Task<IActionResult> Index(string? search, int? genreId, int? artistId, int page = 1)
        {
            const int pageSize = 15;

            var query = _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Album)
                .Include(s => s.Genre)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.Title.Contains(search) || 
                                        (s.Artist != null && s.Artist.Name.Contains(search)));
            }

            if (genreId.HasValue)
            {
                query = query.Where(s => s.GenreId == genreId.Value);
            }

            if (artistId.HasValue)
            {
                query = query.Where(s => s.ArtistId == artistId.Value);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var songs = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new AdminSongListItemViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    ArtistName = s.Artist != null ? s.Artist.Name : "Unknown",
                    AlbumTitle = s.Album != null ? s.Album.Title : null,
                    GenreName = s.Genre != null ? s.Genre.Name : null,
                    Duration = s.Duration,
                    PlayCount = s.PlayCount,
                    LikeCount = s.LikeCount,
                    IsPublished = s.IsPublished,
                    CreatedAt = s.CreatedAt,
                    CoverImageUrl = s.CoverImageUrl
                })
                .ToListAsync();

            // Load filter options
            ViewBag.Genres = new SelectList(await _context.Genres.OrderBy(g => g.Name).ToListAsync(), "Id", "Name", genreId);
            ViewBag.Artists = new SelectList(await _context.Artists.OrderBy(a => a.Name).ToListAsync(), "Id", "Name", artistId);
            ViewBag.Search = search;
            ViewBag.GenreId = genreId;
            ViewBag.ArtistId = artistId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(songs);
        }

        // GET: Admin/Songs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var song = await _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Album)
                .Include(s => s.Genre)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (song == null)
            {
                return NotFound();
            }

            return View(song);
        }

        // GET: Admin/Songs/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View(new AdminSongFormViewModel());
        }

        // POST: Admin/Songs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminSongFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var song = new Song
                {
                    Title = model.Title,
                    ArtistId = model.ArtistId,
                    AlbumId = model.AlbumId,
                    GenreId = model.GenreId,
                    Duration = model.Duration,
                    Lyrics = model.Lyrics,
                    CreatedAt = DateTime.UtcNow
                };

                // Handle cover image upload
                if (model.CoverImage != null)
                {
                    song.CoverImageUrl = await FileHelper.SaveFileAsync(
                        model.CoverImage, 
                        _environment.WebRootPath, 
                        "uploads/covers");
                }
                else
                {
                    song.CoverImageUrl = "/images/default-song.svg";
                }

                // Handle audio file upload
                if (model.AudioFile != null)
                {
                    song.AudioUrl = await FileHelper.SaveFileAsync(
                        model.AudioFile, 
                        _environment.WebRootPath, 
                        "uploads/songs");
                    song.Duration = model.Duration;
                }

                _context.Songs.Add(song);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Song added successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(model.ArtistId, model.AlbumId, model.GenreId);
            return View(model);
        }

        // GET: Admin/Songs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var song = await _context.Songs.FindAsync(id);
            if (song == null)
            {
                return NotFound();
            }

            var model = new AdminSongFormViewModel
            {
                Id = song.Id,
                Title = song.Title,
                ArtistId = song.ArtistId,
                AlbumId = song.AlbumId,
                GenreId = song.GenreId,
                Duration = song.Duration,
                Lyrics = song.Lyrics,
                IsPublished = song.IsPublished,
                CoverImageUrl = song.CoverImageUrl,
                AudioUrl = song.AudioUrl
            };

            await PopulateDropdowns(song.ArtistId, song.AlbumId, song.GenreId);
            return View(model);
        }

        // POST: Admin/Songs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminSongFormViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var song = await _context.Songs.FindAsync(id);
                    if (song == null)
                    {
                        return NotFound();
                    }

                    song.Title = model.Title;
                    song.ArtistId = model.ArtistId;
                    song.AlbumId = model.AlbumId;
                    song.GenreId = model.GenreId;
                    song.Duration = model.Duration;
                    song.Lyrics = model.Lyrics;
                    song.UpdatedAt = DateTime.UtcNow;

                    // Handle cover image upload
                    if (model.CoverImage != null)
                    {
                        // Delete old cover
                        if (!string.IsNullOrEmpty(song.CoverImageUrl))
                        {
                            FileHelper.DeleteFile(_environment.WebRootPath, song.CoverImageUrl);
                        }

                        song.CoverImageUrl = await FileHelper.SaveFileAsync(
                            model.CoverImage, 
                            _environment.WebRootPath, 
                            "uploads/covers");
                    }

                    // Handle audio file upload
                    if (model.AudioFile != null)
                    {
                        // Delete old audio
                        if (!string.IsNullOrEmpty(song.AudioUrl))
                        {
                            FileHelper.DeleteFile(_environment.WebRootPath, song.AudioUrl);
                        }

                        song.AudioUrl = await FileHelper.SaveFileAsync(
                            model.AudioFile, 
                            _environment.WebRootPath, 
                            "uploads/songs");
                    }

                    _context.Update(song);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Song updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SongExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            await PopulateDropdowns(model.ArtistId, model.AlbumId, model.GenreId);
            return View(model);
        }

        // POST: Admin/Songs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
            {
                return NotFound();
            }

            // Delete associated files
            if (!string.IsNullOrEmpty(song.CoverImageUrl))
            {
                FileHelper.DeleteFile(_environment.WebRootPath, song.CoverImageUrl);
            }
            if (!string.IsNullOrEmpty(song.AudioUrl))
            {
                FileHelper.DeleteFile(_environment.WebRootPath, song.AudioUrl);
            }

            _context.Songs.Remove(song);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Song deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Songs/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
            {
                return Json(new { success = false, message = "Song not found" });
            }

            song.IsPublished = !song.IsPublished;
            song.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                isActive = song.IsPublished,
                message = song.IsPublished ? "Song activated" : "Song deactivated" 
            });
        }

        // POST: Admin/Songs/BulkDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["Error"] = "No songs selected for deletion";
                return RedirectToAction(nameof(Index));
            }

            var songs = await _context.Songs.Where(s => ids.Contains(s.Id)).ToListAsync();

            foreach (var song in songs)
            {
                // Delete associated files
                if (!string.IsNullOrEmpty(song.CoverImageUrl))
                {
                    FileHelper.DeleteFile(_environment.WebRootPath, song.CoverImageUrl);
                }
                if (!string.IsNullOrEmpty(song.AudioUrl))
                {
                    FileHelper.DeleteFile(_environment.WebRootPath, song.AudioUrl);
                }
            }

            _context.Songs.RemoveRange(songs);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Deleted {songs.Count} songs";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Songs/GetAlbumsByArtist
        [HttpGet]
        public async Task<IActionResult> GetAlbumsByArtist(int artistId)
        {
            var albums = await _context.Albums
                .Where(a => a.ArtistId == artistId)
                .OrderBy(a => a.Title)
                .Select(a => new { value = a.Id, text = a.Title })
                .ToListAsync();

            return Json(albums);
        }

        private bool SongExists(int id)
        {
            return _context.Songs.Any(e => e.Id == id);
        }

        private async Task PopulateDropdowns(int? artistId = null, int? albumId = null, int? genreId = null)
        {
            ViewBag.Artists = new SelectList(
                await _context.Artists.OrderBy(a => a.Name).ToListAsync(), 
                "Id", "Name", artistId);

            if (artistId.HasValue)
            {
                ViewBag.Albums = new SelectList(
                    await _context.Albums.Where(a => a.ArtistId == artistId.Value).OrderBy(a => a.Title).ToListAsync(),
                    "Id", "Title", albumId);
            }
            else
            {
                ViewBag.Albums = new SelectList(
                    await _context.Albums.OrderBy(a => a.Title).ToListAsync(),
                    "Id", "Title", albumId);
            }

            ViewBag.Genres = new SelectList(
                await _context.Genres.OrderBy(g => g.Name).ToListAsync(), 
                "Id", "Name", genreId);
        }
    }
}

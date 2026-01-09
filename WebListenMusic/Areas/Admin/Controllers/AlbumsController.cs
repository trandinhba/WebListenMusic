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
    public class AlbumsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AlbumsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Admin/Albums
        public async Task<IActionResult> Index(string? search, int? artistId, int? year, int page = 1)
        {
            const int pageSize = 12;

            var query = _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Songs)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => a.Title.Contains(search) || 
                                        (a.Artist != null && a.Artist.Name.Contains(search)));
            }

            if (artistId.HasValue)
            {
                query = query.Where(a => a.ArtistId == artistId.Value);
            }

            if (year.HasValue)
            {
                query = query.Where(a => a.ReleaseDate.HasValue && a.ReleaseDate.Value.Year == year.Value);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var albums = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AdminAlbumListItemViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    ArtistId = a.ArtistId,
                    ArtistName = a.Artist != null ? a.Artist.Name : "Unknown",
                    ReleaseDate = a.ReleaseDate,
                    SongCount = a.Songs.Count,
                    TotalPlays = a.Songs.Sum(s => s.PlayCount),
                    IsPublished = a.IsPublished,
                    CoverImageUrl = a.CoverImageUrl,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            // Load filter options
            ViewBag.Artists = new SelectList(await _context.Artists.OrderBy(a => a.Name).ToListAsync(), "Id", "Name", artistId);
            ViewBag.Years = await _context.Albums
                .Where(a => a.ReleaseDate.HasValue)
                .Select(a => a.ReleaseDate!.Value.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
            ViewBag.Search = search;
            ViewBag.ArtistId = artistId;
            ViewBag.Year = year;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(albums);
        }

        // GET: Admin/Albums/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Songs)
                    .ThenInclude(s => s.Genre)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // GET: Admin/Albums/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Artists = new SelectList(await _context.Artists.OrderBy(a => a.Name).ToListAsync(), "Id", "Name");
            return View(new AdminAlbumFormViewModel());
        }

        // POST: Admin/Albums/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminAlbumFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var album = new Album
                {
                    Title = model.Title,
                    ArtistId = model.ArtistId,
                    ReleaseDate = model.ReleaseDate,
                    Description = model.Description,
                    IsPublished = model.IsPublished,
                    CreatedAt = DateTime.UtcNow
                };

                // Handle cover image upload
                if (model.CoverImage != null)
                {
                    album.CoverImageUrl = await FileHelper.SaveFileAsync(
                        model.CoverImage, 
                        _environment.WebRootPath, 
                        "uploads/albums");
                }

                _context.Albums.Add(album);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Album added successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Artists = new SelectList(await _context.Artists.OrderBy(a => a.Name).ToListAsync(), "Id", "Name", model.ArtistId);
            return View(model);
        }

        // GET: Admin/Albums/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albums.FindAsync(id);
            if (album == null)
            {
                return NotFound();
            }

            var model = new AdminAlbumFormViewModel
            {
                Id = album.Id,
                Title = album.Title,
                ArtistId = album.ArtistId,
                ReleaseDate = album.ReleaseDate,
                Description = album.Description,
                IsPublished = album.IsPublished,
                CoverImageUrl = album.CoverImageUrl
            };

            ViewBag.Artists = new SelectList(await _context.Artists.OrderBy(a => a.Name).ToListAsync(), "Id", "Name", album.ArtistId);
            return View(model);
        }

        // POST: Admin/Albums/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminAlbumFormViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var album = await _context.Albums.FindAsync(id);
                    if (album == null)
                    {
                        return NotFound();
                    }

                    album.Title = model.Title;
                    album.ArtistId = model.ArtistId;
                    album.ReleaseDate = model.ReleaseDate;
                    album.Description = model.Description;
                    album.IsPublished = model.IsPublished;
                    album.UpdatedAt = DateTime.UtcNow;

                    // Handle cover image upload
                    if (model.CoverImage != null)
                    {
                        // Delete old cover
                        if (!string.IsNullOrEmpty(album.CoverImageUrl))
                        {
                            FileHelper.DeleteFile(_environment.WebRootPath, album.CoverImageUrl);
                        }

                        album.CoverImageUrl = await FileHelper.SaveFileAsync(
                            model.CoverImage, 
                            _environment.WebRootPath, 
                            "uploads/albums");
                    }

                    _context.Update(album);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Album updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlbumExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Artists = new SelectList(await _context.Artists.OrderBy(a => a.Name).ToListAsync(), "Id", "Name", model.ArtistId);
            return View(model);
        }

        // POST: Admin/Albums/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var album = await _context.Albums
                .Include(a => a.Songs)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null)
            {
                return NotFound();
            }

            // Check if album has songs
            if (album.Songs.Any())
            {
                TempData["Error"] = "Cannot delete album with songs. Please delete the songs first.";
                return RedirectToAction(nameof(Index));
            }

            // Delete cover image
            if (!string.IsNullOrEmpty(album.CoverImageUrl))
            {
                FileHelper.DeleteFile(_environment.WebRootPath, album.CoverImageUrl);
            }

            _context.Albums.Remove(album);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Album deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Albums/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var album = await _context.Albums.FindAsync(id);
            if (album == null)
            {
                return Json(new { success = false, message = "Album not found" });
            }

            album.IsPublished = !album.IsPublished;
            album.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                isActive = album.IsPublished,
                message = album.IsPublished ? "Album activated" : "Album deactivated" 
            });
        }

        private bool AlbumExists(int id)
        {
            return _context.Albums.Any(e => e.Id == id);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebListenMusic.Helpers;
using WebListenMusic.Models;
using WebListenMusic.Models.ViewModels;

namespace WebListenMusic.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ArtistsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ArtistsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Admin/Artists
        public async Task<IActionResult> Index(string? search, bool? isVerified, int page = 1)
        {
            const int pageSize = 12;

            var query = _context.Artists
                .Include(a => a.Songs)
                .Include(a => a.Albums)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => a.Name.Contains(search));
            }

            if (isVerified.HasValue)
            {
                query = query.Where(a => a.IsVerified == isVerified.Value);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var artists = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AdminArtistListItemViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    ImageUrl = a.ImageUrl,
                    IsVerified = a.IsVerified,
                    SongCount = a.Songs.Count,
                    AlbumCount = a.Albums.Count,
                    TotalPlays = a.Songs.Sum(s => s.PlayCount),
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.IsVerified = isVerified;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(artists);
        }

        // GET: Admin/Artists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artist = await _context.Artists
                .Include(a => a.Songs.OrderByDescending(s => s.PlayCount).Take(10))
                    .ThenInclude(s => s.Genre)
                .Include(a => a.Albums.OrderByDescending(al => al.CreatedAt).Take(5))
                .FirstOrDefaultAsync(a => a.Id == id);

            if (artist == null)
            {
                return NotFound();
            }

            // Get full statistics
            ViewBag.TotalPlays = await _context.Songs.Where(s => s.ArtistId == id).SumAsync(s => s.PlayCount);
            ViewBag.TotalLikes = await _context.Songs.Where(s => s.ArtistId == id).SumAsync(s => s.LikeCount);

            return View(artist);
        }

        // GET: Admin/Artists/Create
        public IActionResult Create()
        {
            return View(new AdminArtistFormViewModel());
        }

        // POST: Admin/Artists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminArtistFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var artist = new Artist
                {
                    Name = model.Name,
                    Bio = model.Bio,
                    IsVerified = model.IsVerified,
                    CreatedAt = DateTime.UtcNow
                };

                // Handle image upload
                if (model.ImageFile != null)
                {
                    artist.ImageUrl = await FileHelper.SaveFileAsync(
                        model.ImageFile,
                        _environment.WebRootPath,
                        "uploads/artists");
                }

                _context.Artists.Add(artist);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Artist added successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Admin/Artists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artist = await _context.Artists.FindAsync(id);
            if (artist == null)
            {
                return NotFound();
            }

            var model = new AdminArtistFormViewModel
            {
                Id = artist.Id,
                Name = artist.Name,
                Bio = artist.Bio,
                ImageUrl = artist.ImageUrl,
                IsVerified = artist.IsVerified
            };

            return View(model);
        }

        // POST: Admin/Artists/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminArtistFormViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var artist = await _context.Artists.FindAsync(id);
                    if (artist == null)
                    {
                        return NotFound();
                    }

                    artist.Name = model.Name;
                    artist.Bio = model.Bio;
                    artist.IsVerified = model.IsVerified;
                    artist.UpdatedAt = DateTime.UtcNow;

                    // Handle image upload
                    if (model.ImageFile != null)
                    {
                        if (!string.IsNullOrEmpty(artist.ImageUrl))
                        {
                            FileHelper.DeleteFile(_environment.WebRootPath, artist.ImageUrl);
                        }

                        artist.ImageUrl = await FileHelper.SaveFileAsync(
                            model.ImageFile,
                            _environment.WebRootPath,
                            "uploads/artists");
                    }

                    _context.Update(artist);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Artist updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArtistExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(model);
        }

        // POST: Admin/Artists/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var artist = await _context.Artists
                .Include(a => a.Songs)
                .Include(a => a.Albums)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (artist == null)
            {
                return NotFound();
            }

            // Check if artist has songs or albums
            if (artist.Songs.Any() || artist.Albums.Any())
            {
                TempData["Error"] = "Cannot delete artist with songs or albums.";
                return RedirectToAction(nameof(Index));
            }

            // Delete image
            if (!string.IsNullOrEmpty(artist.ImageUrl))
            {
                FileHelper.DeleteFile(_environment.WebRootPath, artist.ImageUrl);
            }

            _context.Artists.Remove(artist);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Artist deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Artists/ToggleVerified/5
        [HttpPost]
        public async Task<IActionResult> ToggleVerified(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist == null)
            {
                return Json(new { success = false, message = "Artist not found" });
            }

            artist.IsVerified = !artist.IsVerified;
            artist.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isVerified = artist.IsVerified,
                message = artist.IsVerified ? "Artist verified" : "Artist verification removed"
            });
        }

        private bool ArtistExists(int id)
        {
            return _context.Artists.Any(e => e.Id == id);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebListenMusic.Models;

namespace WebListenMusic.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GenresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GenresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Genres
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            const int pageSize = 15;

            var query = _context.Genres
                .Include(g => g.Songs)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(g => g.Name.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var genres = await query
                .OrderBy(g => g.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(genres);
        }

        // GET: Admin/Genres/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genre = await _context.Genres
                .Include(g => g.Songs)
                    .ThenInclude(s => s.Artist)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (genre == null)
            {
                return NotFound();
            }

            return View(genre);
        }

        // GET: Admin/Genres/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Genres/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Genre genre)
        {
            // Kiểm tra trùng tên
            if (await _context.Genres.AnyAsync(g => g.Name == genre.Name))
            {
                ModelState.AddModelError("Name", "Genre name already exists.");
            }

            if (ModelState.IsValid)
            {
                genre.CreatedAt = DateTime.Now;
                _context.Add(genre);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Genre added successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(genre);
        }

        // GET: Admin/Genres/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound();
            }

            return View(genre);
        }

        // POST: Admin/Genres/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Genre genre)
        {
            if (id != genre.Id)
            {
                return NotFound();
            }

            // Kiểm tra trùng tên (ngoại trừ chính nó)
            if (await _context.Genres.AnyAsync(g => g.Name == genre.Name && g.Id != id))
            {
                ModelState.AddModelError("Name", "Genre name already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingGenre = await _context.Genres.FindAsync(id);
                    if (existingGenre == null)
                    {
                        return NotFound();
                    }

                    existingGenre.Name = genre.Name;
                    existingGenre.Description = genre.Description;
                    existingGenre.Color = genre.Color;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Genre updated successfully!";;
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GenreExists(genre.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(genre);
        }

        // GET: Admin/Genres/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genre = await _context.Genres
                .Include(g => g.Songs)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (genre == null)
            {
                return NotFound();
            }

            return View(genre);
        }

        // POST: Admin/Genres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var genre = await _context.Genres
                .Include(g => g.Songs)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (genre == null)
            {
                return NotFound();
            }

            // Kiểm tra có bài hát không
            if (genre.Songs.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete this genre because it has {genre.Songs.Count} songs.";
                return RedirectToAction(nameof(Index));
            }

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Genre deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool GenreExists(int id)
        {
            return _context.Genres.Any(e => e.Id == id);
        }
    }
}

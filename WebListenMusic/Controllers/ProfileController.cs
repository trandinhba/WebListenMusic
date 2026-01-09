using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebListenMusic.Models;
using WebListenMusic.Models.ViewModels;

namespace WebListenMusic.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;
        private const int PageSize = 20;

        public ProfileController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
        }

        // GET: Profile
        public async Task<IActionResult> Index(string? id)
        {
            ApplicationUser? user;
            bool isOwnProfile;

            if (string.IsNullOrEmpty(id))
            {
                user = await _userManager.GetUserAsync(User);
                isOwnProfile = true;
            }
            else
            {
                user = await _userManager.FindByIdAsync(id);
                isOwnProfile = user?.Id == _userManager.GetUserId(User);
            }

            if (user == null) return NotFound();

            var playlists = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .Where(p => p.UserId == user.Id && (p.IsPublic || isOwnProfile))
                .OrderByDescending(p => p.CreatedAt)
                .Take(6)
                .ToListAsync();

            var viewModel = new ProfileViewModel
            {
                User = user,
                IsOwnProfile = isOwnProfile,
                Playlists = playlists,
                TotalPlaylists = await _context.Playlists.CountAsync(p => p.UserId == user.Id && (p.IsPublic || isOwnProfile))
            };

            return View(viewModel);
        }

        // GET: Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser model, IFormFile? avatarFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.DisplayName = model.DisplayName;
            user.Bio = model.Bio;

            if (avatarFile != null && avatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploadsFolder);

                // Delete old avatar
                if (!string.IsNullOrEmpty(user.AvatarUrl) && !user.AvatarUrl.Contains("default"))
                {
                    var oldPath = Path.Combine(_environment.WebRootPath, user.AvatarUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(avatarFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }

                user.AvatarUrl = $"/uploads/avatars/{fileName}";
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // C?p nh?t Claims
                var existingClaims = await _userManager.GetClaimsAsync(user);
                
                // Xóa claims c?
                var avatarClaim = existingClaims.FirstOrDefault(c => c.Type == "AvatarUrl");
                var displayNameClaim = existingClaims.FirstOrDefault(c => c.Type == "DisplayName");
                
                if (avatarClaim != null)
                    await _userManager.RemoveClaimAsync(user, avatarClaim);
                if (displayNameClaim != null)
                    await _userManager.RemoveClaimAsync(user, displayNameClaim);
                
                // Thêm claims m?i
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("AvatarUrl", user.AvatarUrl ?? "/images/default-avatar.svg"));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("DisplayName", user.DisplayName ?? user.Email!));
                
                // Refresh sign-in ?? c?p nh?t cookie v?i claims m?i
                await _signInManager.RefreshSignInAsync(user);

                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(user);
        }

        // GET: Profile/Settings
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return View(user);
        }

        // GET: Profile/Favorites
        public async Task<IActionResult> Favorites(int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var query = _context.FavoriteSongs
                .Include(f => f.Song)
                    .ThenInclude(s => s!.Artist)
                .Include(f => f.Song)
                    .ThenInclude(s => s!.Genre)
                .Where(f => f.UserId == userId && f.Song != null && f.Song.IsPublished)
                .OrderByDescending(f => f.AddedAt);

            var totalItems = await query.CountAsync();
            var favorites = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewBag.TotalItems = totalItems;

            return View(favorites);
        }

        // POST: Profile/ToggleFavorite
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int songId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { success = false, message = "Please login" });

            var song = await _context.Songs.FindAsync(songId);
            if (song == null) return Json(new { success = false, message = "Song not found" });

            var existing = await _context.FavoriteSongs
                .FirstOrDefaultAsync(f => f.UserId == userId && f.SongId == songId);

            bool isFavorite;
            if (existing != null)
            {
                // Remove from favorites
                _context.FavoriteSongs.Remove(existing);
                song.LikeCount = Math.Max(0, song.LikeCount - 1);
                isFavorite = false;
            }
            else
            {
                // Add to favorites
                _context.FavoriteSongs.Add(new FavoriteSong
                {
                    UserId = userId,
                    SongId = songId,
                    AddedAt = DateTime.UtcNow
                });
                song.LikeCount++;
                isFavorite = true;
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isFavorite = isFavorite,
                likeCount = song.LikeCount,
                message = isFavorite ? "Added to favorites" : "Removed from favorites"
            });
        }

        // GET: Profile/IsFavorite/{songId}
        [HttpGet]
        public async Task<IActionResult> IsFavorite(int songId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { isFavorite = false });

            var exists = await _context.FavoriteSongs
                .AnyAsync(f => f.UserId == userId && f.SongId == songId);

            return Json(new { isFavorite = exists });
        }

        // GET: Profile/History
        public async Task<IActionResult> History(int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var query = _context.ListeningHistories
                .Include(h => h.Song)
                    .ThenInclude(s => s!.Artist)
                .Include(h => h.Song)
                    .ThenInclude(s => s!.Genre)
                .Where(h => h.UserId == userId && h.Song != null && h.Song.IsPublished)
                .OrderByDescending(h => h.ListenedAt);

            var totalItems = await query.CountAsync();
            var history = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewBag.TotalItems = totalItems;

            return View(history);
        }

        // POST: Profile/ClearHistory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearHistory()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var userHistory = await _context.ListeningHistories
                .Where(h => h.UserId == userId)
                .ToListAsync();

            _context.ListeningHistories.RemoveRange(userHistory);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Listening history cleared successfully!";
            return RedirectToAction(nameof(History));
        }
    }
}

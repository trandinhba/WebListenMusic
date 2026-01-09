using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebListenMusic.Models;
using WebListenMusic.Models.ViewModels;

namespace WebListenMusic.Controllers
{
    public class SongsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private const int PageSize = 24;

        public SongsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: Songs
        public async Task<IActionResult> Index(string? search, int? genre, int? artist, string sort = "newest", int page = 1)
        {
            var query = _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Genre)
                .Where(s => s.IsPublished);

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.Title.Contains(search) || 
                                        (s.Artist != null && s.Artist.Name.Contains(search)));
            }

            // Filter by Genre
            if (genre.HasValue)
            {
                query = query.Where(s => s.GenreId == genre.Value);
            }

            // Filter by Artist
            if (artist.HasValue)
            {
                query = query.Where(s => s.ArtistId == artist.Value);
            }

            // Sorting
            query = sort switch
            {
                "trending" => query.OrderByDescending(s => s.PlayCount),
                "popular" => query.OrderByDescending(s => s.LikeCount),
                "oldest" => query.OrderBy(s => s.CreatedAt),
                "title" => query.OrderBy(s => s.Title),
                _ => query.OrderByDescending(s => s.CreatedAt) // newest
            };

            var totalItems = await query.CountAsync();
            var songs = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var viewModel = new SongsViewModel
            {
                Songs = songs,
                Search = search,
                GenreId = genre,
                ArtistId = artist,
                Sort = sort,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize),
                TotalItems = totalItems,
                Genres = await _context.Genres.ToListAsync(),
                Artists = await _context.Artists.Take(20).ToListAsync()
            };

            // Get selected genre/artist name for display
            if (genre.HasValue)
            {
                var selectedGenre = await _context.Genres.FindAsync(genre.Value);
                viewModel.SelectedGenreName = selectedGenre?.Name;
            }
            if (artist.HasValue)
            {
                var selectedArtist = await _context.Artists.FindAsync(artist.Value);
                viewModel.SelectedArtistName = selectedArtist?.Name;
            }

            return View(viewModel);
        }

        // GET: Songs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var song = await _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.Album)
                .Include(s => s.Genre)
                .FirstOrDefaultAsync(s => s.Id == id && s.IsPublished);

            if (song == null) return NotFound();

            // Increment play count
            song.PlayCount++;
            
            // Save to listening history if user is logged in
            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId))
            {
                _context.ListeningHistories.Add(new ListeningHistory
                {
                    UserId = userId,
                    SongId = song.Id,
                    ListenedAt = DateTime.UtcNow
                });
            }
            
            await _context.SaveChangesAsync();

            // Related songs (same artist or genre)
            var relatedSongs = await _context.Songs
                .Include(s => s.Artist)
                .Where(s => s.IsPublished && s.Id != song.Id && 
                           (s.ArtistId == song.ArtistId || s.GenreId == song.GenreId))
                .OrderByDescending(s => s.PlayCount)
                .Take(6)
                .ToListAsync();

            ViewBag.RelatedSongs = relatedSongs;

            return View(song);
        }

        // POST: Songs/Like/5
        [HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null) return NotFound();

            song.LikeCount++;
            await _context.SaveChangesAsync();

            return Json(new { success = true, likeCount = song.LikeCount });
        }

        // GET: Songs/GetSongData/5 (for player)
        [HttpGet]
        public async Task<IActionResult> GetSongData(int id)
        {
            var song = await _context.Songs
                .Include(s => s.Artist)
                .FirstOrDefaultAsync(s => s.Id == id && s.IsPublished);

            if (song == null) return NotFound();

            return Json(new
            {
                id = song.Id,
                title = song.Title,
                artist = song.Artist?.Name ?? "Unknown",
                artistId = song.ArtistId,
                audioUrl = song.AudioUrl,
                coverUrl = song.CoverImageUrl ?? "/images/default-song.svg",
                duration = song.Duration,
                lyrics = song.Lyrics
            });
        }

        // GET: Songs/Download/{id}
        [Authorize]
        public async Task<IActionResult> Download(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null) return NotFound();

            // Check if song has audio file
            if (string.IsNullOrEmpty(song.AudioUrl))
            {
                TempData["Error"] = "This song is not available for download.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Get file path
            var filePath = Path.Combine(_environment.WebRootPath, song.AudioUrl.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "Audio file not found.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Increment download count
            song.DownloadCount++;
            await _context.SaveChangesAsync();

            // Get file name
            var fileName = $"{song.Title}{Path.GetExtension(song.AudioUrl)}";
            var contentType = "audio/mpeg";

            return PhysicalFile(filePath, contentType, fileName);
        }

        #region Rating Actions

        // POST: Songs/Rate
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Rate([FromBody] SongRatingViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Please login to rate" });

            if (model.Rating < 1 || model.Rating > 5)
                return Json(new { success = false, message = "Rating must be between 1 and 5" });

            var existingRating = await _context.SongRatings
                .FirstOrDefaultAsync(r => r.SongId == model.SongId && r.UserId == userId);

            if (existingRating != null)
            {
                // Update existing rating
                existingRating.Rating = model.Rating;
                existingRating.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new rating
                _context.SongRatings.Add(new SongRating
                {
                    SongId = model.SongId,
                    UserId = userId,
                    Rating = model.Rating
                });
            }

            await _context.SaveChangesAsync();

            // Get updated stats
            var stats = await GetRatingStats(model.SongId);
            
            return Json(new { 
                success = true, 
                message = "Rating saved",
                averageRating = stats.AverageRating,
                totalRatings = stats.TotalRatings,
                userRating = model.Rating
            });
        }

        // GET: Songs/GetRating/{songId}
        [HttpGet]
        [Route("Songs/GetRating/{songId}")]
        public async Task<IActionResult> GetRating(int songId)
        {
            var stats = await GetRatingStats(songId);
            
            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId))
            {
                var userRating = await _context.SongRatings
                    .FirstOrDefaultAsync(r => r.SongId == songId && r.UserId == userId);
                stats.UserRating = userRating?.Rating;
            }
            
            return Json(stats);
        }

        private async Task<SongRatingDisplayViewModel> GetRatingStats(int songId)
        {
            var ratings = await _context.SongRatings
                .Where(r => r.SongId == songId)
                .ToListAsync();

            var distribution = new int[5];
            foreach (var r in ratings)
            {
                distribution[r.Rating - 1]++;
            }

            return new SongRatingDisplayViewModel
            {
                AverageRating = ratings.Any() ? Math.Round(ratings.Average(r => r.Rating), 1) : 0,
                TotalRatings = ratings.Count,
                RatingDistribution = distribution
            };
        }

        #endregion

        #region Comment Actions

        // POST: Songs/AddComment
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment([FromBody] SongCommentViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Please login to comment" });

            if (string.IsNullOrWhiteSpace(model.Content))
                return Json(new { success = false, message = "Comment cannot be empty" });

            var comment = new SongComment
            {
                SongId = model.SongId,
                UserId = userId,
                Content = model.Content.Trim()
            };

            _context.SongComments.Add(comment);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(userId);
            
            return Json(new { 
                success = true, 
                message = "Comment added",
                comment = new SongCommentDisplayViewModel
                {
                    Id = comment.Id,
                    UserId = userId,
                    UserName = user?.DisplayName ?? user?.UserName ?? "User",
                    UserAvatar = user?.AvatarUrl,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                    IsEdited = false,
                    IsOwner = true
                }
            });
        }

        // PUT: Songs/EditComment
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> EditComment([FromBody] SongCommentEditViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var comment = await _context.SongComments.FindAsync(model.Id);
            
            if (comment == null)
                return Json(new { success = false, message = "Comment not found" });
            
            if (comment.UserId != userId)
                return Json(new { success = false, message = "You can only edit your own comments" });

            comment.Content = model.Content.Trim();
            comment.UpdatedAt = DateTime.UtcNow;
            comment.IsEdited = true;

            await _context.SaveChangesAsync();
            
            return Json(new { success = true, message = "Comment updated" });
        }

        // DELETE: Songs/DeleteComment/{id}
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = _userManager.GetUserId(User);
            var comment = await _context.SongComments.FindAsync(id);
            
            if (comment == null)
                return Json(new { success = false, message = "Comment not found" });
            
            if (comment.UserId != userId)
                return Json(new { success = false, message = "You can only delete your own comments" });

            _context.SongComments.Remove(comment);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true, message = "Comment deleted" });
        }

        // GET: Songs/GetComments/{songId}
        [HttpGet]
        [Route("Songs/GetComments/{songId}")]
        public async Task<IActionResult> GetComments(int songId, int page = 1, int pageSize = 10)
        {
            var userId = _userManager.GetUserId(User);
            
            var query = _context.SongComments
                .Include(c => c.User)
                .Where(c => c.SongId == songId)
                .OrderByDescending(c => c.CreatedAt);

            var totalComments = await query.CountAsync();
            var comments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new SongCommentDisplayViewModel
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    UserName = c.User != null ? (c.User.DisplayName ?? c.User.UserName ?? "User") : "User",
                    UserAvatar = c.User != null ? c.User.AvatarUrl : null,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    IsEdited = c.IsEdited,
                    IsOwner = c.UserId == userId
                })
                .ToListAsync();

            return Json(new SongCommentsListViewModel
            {
                SongId = songId,
                Comments = comments,
                TotalComments = totalComments,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalComments / (double)pageSize)
            });
        }

        #endregion
    }
}

# Task 18: Bình luận và Đánh giá Bài hát

## Mục tiêu
Thêm tính năng cho phép user bình luận và đánh giá (rating) bài hát.

## Tính năng chính

### 1. Đánh giá bài hát (Rating)
- User có thể đánh giá bài hát từ 1-5 sao
- Hiển thị điểm trung bình và số lượt đánh giá
- Mỗi user chỉ được đánh giá 1 lần/bài hát (có thể sửa)

### 2. Bình luận (Comments)
- User có thể viết bình luận cho bài hát
- Hiển thị danh sách bình luận theo thời gian
- User có thể sửa/xóa bình luận của mình
- Hiển thị avatar, tên user, thời gian đăng

---

## Phần 1: Database Models

### 1.1 Model SongRating
```csharp
// Models/SongRating.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebListenMusic.Models
{
    public class SongRating
    {
        public int Id { get; set; }

        [Required]
        public int SongId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; } // 1-5 stars

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("SongId")]
        public virtual Song? Song { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
```

### 1.2 Model SongComment
```csharp
// Models/SongComment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebListenMusic.Models
{
    public class SongComment
    {
        public int Id { get; set; }

        [Required]
        public int SongId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public bool IsEdited { get; set; } = false;

        // Navigation properties
        [ForeignKey("SongId")]
        public virtual Song? Song { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
```

### 1.3 Cập nhật Song Model
```csharp
// Thêm vào Models/Song.cs
public virtual ICollection<SongRating>? Ratings { get; set; }
public virtual ICollection<SongComment>? Comments { get; set; }

// Computed properties (không lưu DB)
[NotMapped]
public double AverageRating => Ratings?.Any() == true 
    ? Math.Round(Ratings.Average(r => r.Rating), 1) 
    : 0;

[NotMapped]
public int RatingCount => Ratings?.Count ?? 0;

[NotMapped]
public int CommentCount => Comments?.Count ?? 0;
```

### 1.4 Cập nhật ApplicationUser Model
```csharp
// Thêm vào Models/ApplicationUser.cs
public virtual ICollection<SongRating>? SongRatings { get; set; }
public virtual ICollection<SongComment>? SongComments { get; set; }
```

### 1.5 Cập nhật DbContext
```csharp
// Data/ApplicationDbContext.cs
public DbSet<SongRating> SongRatings { get; set; }
public DbSet<SongComment> SongComments { get; set; }

// Trong OnModelCreating
modelBuilder.Entity<SongRating>()
    .HasIndex(r => new { r.SongId, r.UserId })
    .IsUnique(); // Mỗi user chỉ rate 1 lần/bài
```

---

## Phần 2: ViewModels

### 2.1 Rating ViewModel
```csharp
// Models/ViewModels/CustomerViewModels.cs

public class SongRatingViewModel
{
    public int SongId { get; set; }
    public int Rating { get; set; }
}

public class SongRatingDisplayViewModel
{
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public int[] RatingDistribution { get; set; } = new int[5]; // Count per star
    public int? UserRating { get; set; } // Current user's rating
}
```

### 2.2 Comment ViewModel
```csharp
public class SongCommentViewModel
{
    public int SongId { get; set; }
    
    [Required(ErrorMessage = "Please enter a comment")]
    [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
    public string Content { get; set; } = string.Empty;
}

public class SongCommentDisplayViewModel
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsEdited { get; set; }
    public bool IsOwner { get; set; } // True if current user owns this comment
}

public class SongCommentsListViewModel
{
    public int SongId { get; set; }
    public List<SongCommentDisplayViewModel> Comments { get; set; } = new();
    public int TotalComments { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
```

---

## Phần 3: Controller Actions

### 3.1 SongsController - Rating Actions
```csharp
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
```

### 3.2 SongsController - Comment Actions
```csharp
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
```

---

## Phần 4: Views

### 4.1 Cập nhật Songs/Details.cshtml
Thêm section Rating và Comments vào trang chi tiết bài hát:

```html
<!-- Rating Section -->
<div class="rating-section">
    <h3><i class="bi bi-star me-2"></i>Rating</h3>
    <div class="rating-display">
        <div class="rating-average">
            <span class="rating-number" id="avgRating">0</span>
            <div class="rating-stars" id="avgStars">
                <!-- Stars will be rendered by JS -->
            </div>
            <span class="rating-count">(<span id="totalRatings">0</span> ratings)</span>
        </div>
        
        @if (User.Identity?.IsAuthenticated == true)
        {
            <div class="user-rating">
                <p>Your rating:</p>
                <div class="star-rating" id="userRating">
                    @for (int i = 1; i <= 5; i++)
                    {
                        <button class="star-btn" data-rating="@i" onclick="rateSong(@Model.Id, @i)">
                            <i class="bi bi-star"></i>
                        </button>
                    }
                </div>
            </div>
        }
        else
        {
            <p class="text-muted">
                <a asp-controller="Account" asp-action="Login">Login</a> to rate this song
            </p>
        }
    </div>
</div>

<!-- Comments Section -->
<div class="comments-section">
    <h3><i class="bi bi-chat-dots me-2"></i>Comments (<span id="commentCount">0</span>)</h3>
    
    @if (User.Identity?.IsAuthenticated == true)
    {
        <div class="comment-form">
            <div class="comment-input-wrapper">
                <img src="@(((ApplicationUser)await UserManager.GetUserAsync(User))?.AvatarUrl ?? "/images/default-avatar.svg")" 
                     alt="Avatar" class="comment-avatar" />
                <textarea id="commentInput" class="form-control" 
                          placeholder="Write a comment..." maxlength="1000"></textarea>
            </div>
            <div class="comment-form-actions">
                <span class="char-count"><span id="charCount">0</span>/1000</span>
                <button class="btn btn-primary btn-sm" onclick="addComment(@Model.Id)">
                    <i class="bi bi-send me-1"></i>Post
                </button>
            </div>
        </div>
    }
    else
    {
        <p class="text-muted">
            <a asp-controller="Account" asp-action="Login">Login</a> to leave a comment
        </p>
    }
    
    <div class="comments-list" id="commentsList">
        <!-- Comments will be loaded by JS -->
    </div>
    
    <div class="comments-pagination" id="commentsPagination">
        <!-- Pagination will be rendered by JS -->
    </div>
</div>
```

---

## Phần 5: JavaScript

### 5.1 Rating Functions
```javascript
// Rating functionality
let currentUserRating = 0;

async function loadRating(songId) {
    try {
        const response = await fetch(`/Songs/GetRating/${songId}`);
        if (response.ok) {
            const data = await response.json();
            updateRatingDisplay(data);
        }
    } catch (err) {
        console.error('Error loading rating:', err);
    }
}

function updateRatingDisplay(data) {
    document.getElementById('avgRating').textContent = data.averageRating.toFixed(1);
    document.getElementById('totalRatings').textContent = data.totalRatings;
    
    // Render average stars
    const avgStars = document.getElementById('avgStars');
    avgStars.innerHTML = renderStars(data.averageRating);
    
    // Update user rating if logged in
    if (data.userRating) {
        currentUserRating = data.userRating;
        highlightUserStars(data.userRating);
    }
}

function renderStars(rating) {
    let html = '';
    for (let i = 1; i <= 5; i++) {
        if (i <= rating) {
            html += '<i class="bi bi-star-fill text-warning"></i>';
        } else if (i - 0.5 <= rating) {
            html += '<i class="bi bi-star-half text-warning"></i>';
        } else {
            html += '<i class="bi bi-star text-warning"></i>';
        }
    }
    return html;
}

function highlightUserStars(rating) {
    const stars = document.querySelectorAll('#userRating .star-btn');
    stars.forEach((star, index) => {
        const icon = star.querySelector('i');
        if (index < rating) {
            icon.classList.remove('bi-star');
            icon.classList.add('bi-star-fill');
            star.classList.add('active');
        } else {
            icon.classList.remove('bi-star-fill');
            icon.classList.add('bi-star');
            star.classList.remove('active');
        }
    });
}

async function rateSong(songId, rating) {
    try {
        const response = await fetch('/Songs/Rate', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ songId, rating })
        });
        
        if (response.ok) {
            const data = await response.json();
            if (data.success) {
                updateRatingDisplay(data);
                showToast('Rating saved!', 'success');
            } else {
                showToast(data.message, 'warning');
            }
        }
    } catch (err) {
        console.error('Error rating song:', err);
        showToast('Error saving rating', 'error');
    }
}

// Star hover effect
document.querySelectorAll('#userRating .star-btn').forEach((star, index) => {
    star.addEventListener('mouseenter', () => {
        highlightUserStars(index + 1);
    });
    
    star.addEventListener('mouseleave', () => {
        highlightUserStars(currentUserRating);
    });
});
```

### 5.2 Comment Functions
```javascript
// Comment functionality
let currentCommentPage = 1;

async function loadComments(songId, page = 1) {
    try {
        const response = await fetch(`/Songs/GetComments/${songId}?page=${page}`);
        if (response.ok) {
            const data = await response.json();
            renderComments(data);
            currentCommentPage = page;
        }
    } catch (err) {
        console.error('Error loading comments:', err);
    }
}

function renderComments(data) {
    document.getElementById('commentCount').textContent = data.totalComments;
    
    const container = document.getElementById('commentsList');
    
    if (data.comments.length === 0) {
        container.innerHTML = `
            <div class="empty-state text-center py-4">
                <i class="bi bi-chat-dots fs-1 text-muted"></i>
                <p class="text-muted mt-2">No comments yet. Be the first to comment!</p>
            </div>
        `;
        return;
    }
    
    container.innerHTML = data.comments.map(comment => `
        <div class="comment-item" id="comment-${comment.id}">
            <img src="${comment.userAvatar || '/images/default-avatar.svg'}" 
                 alt="Avatar" class="comment-avatar" />
            <div class="comment-content">
                <div class="comment-header">
                    <span class="comment-author">${escapeHtml(comment.userName)}</span>
                    <span class="comment-time">${formatTimeAgo(comment.createdAt)}</span>
                    ${comment.isEdited ? '<span class="comment-edited">(edited)</span>' : ''}
                </div>
                <p class="comment-text" id="comment-text-${comment.id}">${escapeHtml(comment.content)}</p>
                ${comment.isOwner ? `
                    <div class="comment-actions">
                        <button class="btn btn-link btn-sm" onclick="editComment(${comment.id})">
                            <i class="bi bi-pencil"></i> Edit
                        </button>
                        <button class="btn btn-link btn-sm text-danger" onclick="deleteComment(${comment.id})">
                            <i class="bi bi-trash"></i> Delete
                        </button>
                    </div>
                ` : ''}
            </div>
        </div>
    `).join('');
    
    // Render pagination
    renderCommentsPagination(data);
}

function renderCommentsPagination(data) {
    if (data.totalPages <= 1) {
        document.getElementById('commentsPagination').innerHTML = '';
        return;
    }
    
    let html = '<nav><ul class="pagination pagination-sm justify-content-center">';
    
    for (let i = 1; i <= data.totalPages; i++) {
        html += `
            <li class="page-item ${i === data.currentPage ? 'active' : ''}">
                <button class="page-link" onclick="loadComments(${data.songId}, ${i})">${i}</button>
            </li>
        `;
    }
    
    html += '</ul></nav>';
    document.getElementById('commentsPagination').innerHTML = html;
}

async function addComment(songId) {
    const input = document.getElementById('commentInput');
    const content = input.value.trim();
    
    if (!content) {
        showToast('Please enter a comment', 'warning');
        return;
    }
    
    try {
        const response = await fetch('/Songs/AddComment', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ songId, content })
        });
        
        if (response.ok) {
            const data = await response.json();
            if (data.success) {
                input.value = '';
                document.getElementById('charCount').textContent = '0';
                loadComments(songId, 1); // Reload first page
                showToast('Comment added!', 'success');
            } else {
                showToast(data.message, 'warning');
            }
        }
    } catch (err) {
        console.error('Error adding comment:', err);
        showToast('Error adding comment', 'error');
    }
}

async function editComment(commentId) {
    const textEl = document.getElementById(`comment-text-${commentId}`);
    const currentText = textEl.textContent;
    
    const newText = prompt('Edit your comment:', currentText);
    if (newText === null || newText.trim() === currentText) return;
    
    try {
        const response = await fetch('/Songs/EditComment', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ id: commentId, content: newText.trim() })
        });
        
        if (response.ok) {
            const data = await response.json();
            if (data.success) {
                textEl.textContent = newText.trim();
                showToast('Comment updated!', 'success');
            } else {
                showToast(data.message, 'warning');
            }
        }
    } catch (err) {
        console.error('Error editing comment:', err);
        showToast('Error editing comment', 'error');
    }
}

async function deleteComment(commentId) {
    if (!confirm('Are you sure you want to delete this comment?')) return;
    
    try {
        const response = await fetch(`/Songs/DeleteComment/${commentId}`, {
            method: 'DELETE'
        });
        
        if (response.ok) {
            const data = await response.json();
            if (data.success) {
                document.getElementById(`comment-${commentId}`).remove();
                showToast('Comment deleted!', 'success');
            } else {
                showToast(data.message, 'warning');
            }
        }
    } catch (err) {
        console.error('Error deleting comment:', err);
        showToast('Error deleting comment', 'error');
    }
}

// Character counter
document.getElementById('commentInput')?.addEventListener('input', function() {
    document.getElementById('charCount').textContent = this.value.length;
});

// Helper functions
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function formatTimeAgo(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const seconds = Math.floor((now - date) / 1000);
    
    if (seconds < 60) return 'Just now';
    if (seconds < 3600) return `${Math.floor(seconds / 60)} minutes ago`;
    if (seconds < 86400) return `${Math.floor(seconds / 3600)} hours ago`;
    if (seconds < 604800) return `${Math.floor(seconds / 86400)} days ago`;
    
    return date.toLocaleDateString();
}
```

---

## Phần 6: CSS Styles

```css
/* Rating Section */
.rating-section {
    background: var(--ml-surface);
    border-radius: var(--ml-radius-lg);
    padding: 24px;
    margin-bottom: 24px;
}

.rating-display {
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.rating-average {
    display: flex;
    align-items: center;
    gap: 12px;
}

.rating-number {
    font-size: 3rem;
    font-weight: 700;
    color: var(--ml-text);
}

.rating-stars {
    display: flex;
    gap: 4px;
    font-size: 1.5rem;
}

.rating-count {
    color: var(--ml-text-muted);
}

.user-rating {
    padding-top: 16px;
    border-top: 1px solid var(--ml-border);
}

.star-rating {
    display: flex;
    gap: 8px;
}

.star-btn {
    background: none;
    border: none;
    font-size: 1.75rem;
    color: var(--ml-text-muted);
    cursor: pointer;
    transition: var(--ml-transition);
    padding: 4px;
}

.star-btn:hover,
.star-btn.active {
    color: #ffc107;
}

.star-btn i {
    transition: var(--ml-transition);
}

/* Comments Section */
.comments-section {
    background: var(--ml-surface);
    border-radius: var(--ml-radius-lg);
    padding: 24px;
}

.comment-form {
    margin-bottom: 24px;
}

.comment-input-wrapper {
    display: flex;
    gap: 12px;
    margin-bottom: 12px;
}

.comment-avatar {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    object-fit: cover;
    flex-shrink: 0;
}

.comment-form textarea {
    resize: none;
    min-height: 80px;
    background: var(--ml-bg);
    border-color: var(--ml-border);
    color: var(--ml-text);
}

.comment-form-actions {
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.char-count {
    font-size: 0.875rem;
    color: var(--ml-text-muted);
}

.comments-list {
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.comment-item {
    display: flex;
    gap: 12px;
    padding: 16px;
    background: var(--ml-bg);
    border-radius: var(--ml-radius);
}

.comment-content {
    flex: 1;
    min-width: 0;
}

.comment-header {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 8px;
}

.comment-author {
    font-weight: 600;
    color: var(--ml-text);
}

.comment-time {
    font-size: 0.8125rem;
    color: var(--ml-text-muted);
}

.comment-edited {
    font-size: 0.75rem;
    color: var(--ml-text-muted);
    font-style: italic;
}

.comment-text {
    color: var(--ml-text);
    margin-bottom: 8px;
    line-height: 1.6;
    word-wrap: break-word;
}

.comment-actions {
    display: flex;
    gap: 8px;
}

.comment-actions .btn-link {
    padding: 0;
    font-size: 0.8125rem;
}
```

---

## Phần 7: Migration

```bash
# Tạo migration
dotnet ef migrations add AddCommentsAndRatings

# Áp dụng migration
dotnet ef database update
```

---

## Testing Checklist

### Rating
- [ ] User có thể đánh giá 1-5 sao
- [ ] Điểm trung bình hiển thị đúng
- [ ] User có thể thay đổi rating
- [ ] Hover effect trên stars hoạt động
- [ ] User chưa login thấy prompt login

### Comments
- [ ] User có thể thêm comment
- [ ] Comment hiển thị đúng thông tin
- [ ] User có thể sửa comment của mình
- [ ] User có thể xóa comment của mình
- [ ] Pagination hoạt động
- [ ] Character counter hoạt động
- [ ] User chưa login thấy prompt login

---

## Tổng kết Files cần tạo/sửa

| File | Loại | Mô tả |
|------|------|-------|
| `Models/SongRating.cs` | Tạo mới | Model cho rating |
| `Models/SongComment.cs` | Tạo mới | Model cho comment |
| `Models/Song.cs` | Sửa | Thêm navigation properties |
| `Models/ApplicationUser.cs` | Sửa | Thêm navigation properties |
| `Models/ViewModels/CustomerViewModels.cs` | Sửa | Thêm ViewModels |
| `Data/ApplicationDbContext.cs` | Sửa | Thêm DbSets |
| `Controllers/SongsController.cs` | Sửa | Thêm actions |
| `Views/Songs/Details.cshtml` | Sửa | Thêm UI sections |
| `wwwroot/js/site.js` | Sửa | Thêm JS functions |
| `wwwroot/css/site.css` | Sửa | Thêm CSS styles |

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebListenMusic.Models
{
    /// <summary>
    /// Model bình luận bài hát - Lưu comment của user cho bài hát
    /// </summary>
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

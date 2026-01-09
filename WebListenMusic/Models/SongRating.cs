using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebListenMusic.Models
{
    /// <summary>
    /// Model đánh giá bài hát - Lưu rating 1-5 sao từ user
    /// </summary>
    public class SongRating
    {
        public int Id { get; set; }

        [Required]
        public int SongId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
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

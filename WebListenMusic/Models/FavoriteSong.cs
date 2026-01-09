using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebListenMusic.Models
{
    /// <summary>
    /// Model lưu bài hát yêu thích của người dùng
    /// </summary>
    public class FavoriteSong
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int SongId { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("SongId")]
        public virtual Song? Song { get; set; }
    }
}

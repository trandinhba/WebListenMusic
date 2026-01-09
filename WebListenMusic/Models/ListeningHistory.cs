using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebListenMusic.Models
{
    /// <summary>
    /// Model lưu lịch sử nghe nhạc của người dùng
    /// </summary>
    public class ListeningHistory
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int SongId { get; set; }

        public DateTime ListenedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời lượng đã nghe (giây) - có thể null nếu chưa track
        /// </summary>
        public int? DurationListened { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("SongId")]
        public virtual Song? Song { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebListenMusic.Models
{
    public class PlaylistSong
    {
        public int Id { get; set; }

        [Display(Name = "Thứ tự")]
        public int Order { get; set; } = 0;

        [Display(Name = "Ngày thêm")]
        public DateTime AddedAt { get; set; } = DateTime.Now;

        // Foreign keys
        public int PlaylistId { get; set; }
        public int SongId { get; set; }

        // Navigation properties
        [ForeignKey("PlaylistId")]
        public virtual Playlist? Playlist { get; set; }

        [ForeignKey("SongId")]
        public virtual Song? Song { get; set; }
    }
}

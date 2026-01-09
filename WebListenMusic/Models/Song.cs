using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebListenMusic.Models
{
    public class Song
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Song title is required")]
        [StringLength(200, ErrorMessage = "Song title cannot exceed 200 characters")]
        [Display(Name = "Song Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Audio File")]
        public string? AudioUrl { get; set; }

        [Display(Name = "Cover Image")]
        public string? CoverImageUrl { get; set; }

        [Display(Name = "Duration (seconds)")]
        public int Duration { get; set; } = 0;

        [StringLength(5000)]
        [Display(Name = "Lyrics")]
        public string? Lyrics { get; set; }

        [Display(Name = "Play Count")]
        public int PlayCount { get; set; } = 0;

        [Display(Name = "Like Count")]
        public int LikeCount { get; set; } = 0;

        [Display(Name = "Download Count")]
        public int DownloadCount { get; set; } = 0;

        [Display(Name = "Published")]
        public bool IsPublished { get; set; } = true;

        [Display(Name = "Featured")]
        public bool IsFeatured { get; set; } = false;

        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime? ReleaseDate { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        // Foreign keys
        [Display(Name = "Artist")]
        public int? ArtistId { get; set; }

        [Display(Name = "Album")]
        public int? AlbumId { get; set; }

        [Display(Name = "Genre")]
        public int? GenreId { get; set; }

        // Navigation properties
        [ForeignKey("ArtistId")]
        public virtual Artist? Artist { get; set; }

        [ForeignKey("AlbumId")]
        public virtual Album? Album { get; set; }

        [ForeignKey("GenreId")]
        public virtual Genre? Genre { get; set; }

        public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();

        // Rating và Comments
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
    }
}

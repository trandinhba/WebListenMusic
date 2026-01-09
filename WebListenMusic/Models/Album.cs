using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebListenMusic.Models
{
    public class Album
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Album title is required")]
        [StringLength(200, ErrorMessage = "Album title cannot exceed 200 characters")]
        [Display(Name = "Album Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Cover Image")]
        public string? CoverImageUrl { get; set; }

        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime? ReleaseDate { get; set; }

        [Display(Name = "Play Count")]
        public int PlayCount { get; set; } = 0;

        [Display(Name = "Like Count")]
        public int LikeCount { get; set; } = 0;

        [Display(Name = "Published")]
        public bool IsPublished { get; set; } = true;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        // Foreign keys
        [Display(Name = "Artist")]
        public int? ArtistId { get; set; }

        // Navigation properties
        [ForeignKey("ArtistId")]
        public virtual Artist? Artist { get; set; }

        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
    }
}

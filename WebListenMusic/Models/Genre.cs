using System.ComponentModel.DataAnnotations;

namespace WebListenMusic.Models
{
    public class Genre
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Genre name is required")]
        [StringLength(100, ErrorMessage = "Genre name cannot exceed 100 characters")]
        [Display(Name = "Genre Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Display Color")]
        public string? Color { get; set; } = "#1f6feb";

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace WebListenMusic.Models
{
    public class Artist
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Artist name is required")]
        [StringLength(200, ErrorMessage = "Artist name cannot exceed 200 characters")]
        [Display(Name = "Artist Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        [Display(Name = "Biography")]
        public string? Bio { get; set; }

        [Display(Name = "Profile Image")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Country")]
        [StringLength(100)]
        public string? Country { get; set; }

        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Verified")]
        public bool IsVerified { get; set; } = false;

        [Display(Name = "Followers Count")]
        public int FollowersCount { get; set; } = 0;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
        public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
    }
}

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebListenMusic.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        [Display(Name = "Display Name")]
        public string? DisplayName { get; set; }

        [StringLength(500)]
        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        [Display(Name = "Avatar")]
        public string? AvatarUrl { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(20)]
        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        [StringLength(100)]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Premium Account")]
        public bool IsPremium { get; set; } = false;

        [Display(Name = "Premium Expiry Date")]
        public DateTime? PremiumExpiryDate { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Last Login")]
        public DateTime? LastLoginAt { get; set; }

        // Navigation properties
        public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
        public virtual ICollection<SongRating>? SongRatings { get; set; }
        public virtual ICollection<SongComment>? SongComments { get; set; }
    }
}

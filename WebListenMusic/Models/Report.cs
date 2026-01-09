using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebListenMusic.Models
{
    public enum ReportType
    {
        [Display(Name = "Copyright Violation")]
        Copyright,
        [Display(Name = "Inappropriate Content")]
        InappropriateContent,
        [Display(Name = "Technical Issue")]
        TechnicalIssue,
        [Display(Name = "Spam")]
        Spam,
        [Display(Name = "Other")]
        Other
    }

    public enum ReportStatus
    {
        [Display(Name = "Pending")]
        Pending,
        [Display(Name = "In Progress")]
        InProgress,
        [Display(Name = "Resolved")]
        Resolved,
        [Display(Name = "Dismissed")]
        Dismissed
    }

    public class Report
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        [StringLength(2000)]
        [Display(Name = "Content")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Report Type")]
        public ReportType Type { get; set; } = ReportType.Other;

        [Display(Name = "Status")]
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        [StringLength(1000)]
        [Display(Name = "Admin Note")]
        public string? AdminNote { get; set; }

        [Display(Name = "Related Song ID")]
        public int? RelatedSongId { get; set; }

        [Display(Name = "Related Album ID")]
        public int? RelatedAlbumId { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Resolved At")]
        public DateTime? ResolvedAt { get; set; }

        // Foreign key
        [Required]
        public string UserId { get; set; } = string.Empty;

        public string? ResolvedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("ResolvedByUserId")]
        public virtual ApplicationUser? ResolvedByUser { get; set; }

        [ForeignKey("RelatedSongId")]
        public virtual Song? RelatedSong { get; set; }

        [ForeignKey("RelatedAlbumId")]
        public virtual Album? RelatedAlbum { get; set; }
    }
}

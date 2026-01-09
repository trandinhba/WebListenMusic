using Microsoft.AspNetCore.Http;

namespace WebListenMusic.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalSongs { get; set; }
        public int TotalAlbums { get; set; }
        public int TotalArtists { get; set; }
        public int TotalUsers { get; set; }
        public int TotalPlaylists { get; set; }
        public int PendingReports { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewSongsThisMonth { get; set; }

        public List<Song> RecentSongs { get; set; } = new List<Song>();
        public List<ApplicationUser> RecentUsers { get; set; } = new List<ApplicationUser>();
        public List<Report> RecentReports { get; set; } = new List<Report>();
        public List<Song> TopSongs { get; set; } = new List<Song>();

        // Chart data
        public List<int> SongUploadsPerMonth { get; set; } = new List<int>();
        public List<int> UserRegistrationsPerMonth { get; set; } = new List<int>();
        public List<string> MonthLabels { get; set; } = new List<string>();
    }

    #region Song ViewModels

    // Song list item for admin list display
    public class AdminSongListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ArtistName { get; set; }
        public string? AlbumTitle { get; set; }
        public string? GenreName { get; set; }
        public int Duration { get; set; }
        public int PlayCount { get; set; }
        public int LikeCount { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CoverImageUrl { get; set; }
    }

    // Song form for Create/Edit
    public class AdminSongFormViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? ArtistId { get; set; }
        public int? AlbumId { get; set; }
        public int? GenreId { get; set; }
        public int Duration { get; set; }
        public string? Lyrics { get; set; }
        public bool IsPublished { get; set; } = true;
        public string? CoverImageUrl { get; set; }
        public string? AudioUrl { get; set; }
        
        public IFormFile? AudioFile { get; set; }
        public IFormFile? CoverImage { get; set; }
    }

    #endregion

    #region Album ViewModels

    // Album list item for admin list display
    public class AdminAlbumListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? ArtistId { get; set; }
        public string? ArtistName { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int SongCount { get; set; }
        public int TotalPlays { get; set; }
        public bool IsPublished { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Album form for Create/Edit
    public class AdminAlbumFormViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? ArtistId { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Description { get; set; }
        public bool IsPublished { get; set; } = true;
        public string? CoverImageUrl { get; set; }
        public IFormFile? CoverImage { get; set; }
    }

    #endregion

    #region Artist ViewModels

    // Artist list item for admin list display
    public class AdminArtistListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int SongCount { get; set; }
        public int AlbumCount { get; set; }
        public int TotalPlays { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Artist form for Create/Edit
    public class AdminArtistFormViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsVerified { get; set; }
        public IFormFile? ImageFile { get; set; }
    }

    #endregion

    #region User ViewModels

    // User list item for admin display
    public class AdminUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsPremium { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    #endregion

    #region Report ViewModels

    public class AdminReportViewModel
    {
        public Report Report { get; set; } = new Report();
        public string? ActionNote { get; set; }
    }

    #endregion

    #region Legacy ViewModels (for backwards compatibility)

    public class AdminSongViewModel
    {
        public Song Song { get; set; } = new Song();
        public List<Artist> Artists { get; set; } = new List<Artist>();
        public List<Album> Albums { get; set; } = new List<Album>();
        public List<Genre> Genres { get; set; } = new List<Genre>();
        public IFormFile? AudioFile { get; set; }
        public IFormFile? CoverFile { get; set; }
    }

    public class AdminAlbumViewModel
    {
        public Album Album { get; set; } = new Album();
        public List<Artist> Artists { get; set; } = new List<Artist>();
        public List<Song> AvailableSongs { get; set; } = new List<Song>();
        public List<int> SelectedSongIds { get; set; } = new List<int>();
        public IFormFile? CoverFile { get; set; }
    }

    public class AdminArtistViewModel
    {
        public Artist Artist { get; set; } = new Artist();
        public IFormFile? ImageFile { get; set; }
    }

    public class AdminSongListViewModel
    {
        public List<Song> Songs { get; set; } = new List<Song>();
        public string? SearchTerm { get; set; }
        public int? GenreFilter { get; set; }
        public int? ArtistFilter { get; set; }
        public List<Genre> Genres { get; set; } = new List<Genre>();
        public List<Artist> Artists { get; set; } = new List<Artist>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }

    #endregion
}

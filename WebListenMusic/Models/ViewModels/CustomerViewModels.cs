using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebListenMusic.Models.ViewModels
{
    #region Home ViewModels
    
    public class HomeViewModel
    {
        public IEnumerable<Song> FeaturedSongs { get; set; } = new List<Song>();
        public IEnumerable<Song> TrendingSongs { get; set; } = new List<Song>();
        public IEnumerable<Song> NewSongs { get; set; } = new List<Song>();
        public IEnumerable<Album> PopularAlbums { get; set; } = new List<Album>();
        public IEnumerable<Artist> TopArtists { get; set; } = new List<Artist>();
        public IEnumerable<Genre> Genres { get; set; } = new List<Genre>();
        public IEnumerable<Song> NewReleaseSongs { get; set; } = new List<Song>();
        public IEnumerable<Album> NewAlbums { get; set; } = new List<Album>();
        public IEnumerable<Song> RecentlyPlayed { get; set; } = new List<Song>();
    }
    
    #endregion
    
    #region Search ViewModels
    
    public class SearchViewModel
    {
        public string? Query { get; set; }
        public string Type { get; set; } = "all";
        public IEnumerable<Song> Songs { get; set; } = new List<Song>();
        public IEnumerable<Album> Albums { get; set; } = new List<Album>();
        public IEnumerable<Artist> Artists { get; set; } = new List<Artist>();
        public IEnumerable<Playlist> Playlists { get; set; } = new List<Playlist>();
        public int TotalSongs { get; set; }
        public int TotalAlbums { get; set; }
        public int TotalArtists { get; set; }
        public int TotalPlaylists { get; set; }
        
        public bool HasResults => Songs.Any() || Albums.Any() || Artists.Any() || Playlists.Any();
    }
    
    #endregion
    
    #region Songs ViewModels
    
    public class SongsViewModel
    {
        public IEnumerable<Song> Songs { get; set; } = new List<Song>();
        public string? Search { get; set; }
        public int? GenreId { get; set; }
        public int? ArtistId { get; set; }
        public string Sort { get; set; } = "newest";
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        
        public IEnumerable<Genre> Genres { get; set; } = new List<Genre>();
        public IEnumerable<Artist> Artists { get; set; } = new List<Artist>();
        
        public string? SelectedGenreName { get; set; }
        public string? SelectedArtistName { get; set; }
    }
    
    public class SongListViewModel
    {
        public List<Song> Songs { get; set; } = new List<Song>();
        public List<Genre> Genres { get; set; } = new List<Genre>();
        public int? SelectedGenreId { get; set; }
        public string? SortBy { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 20;
    }
    
    #endregion
    
    #region Albums ViewModels
    
    public class AlbumsViewModel
    {
        public IEnumerable<Album> Albums { get; set; } = new List<Album>();
        public string? Search { get; set; }
        public int? ArtistId { get; set; }
        public string Sort { get; set; } = "newest";
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        
        public IEnumerable<Artist> Artists { get; set; } = new List<Artist>();
        public string? SelectedArtistName { get; set; }
    }
    
    public class AlbumListViewModel
    {
        public List<Album> Albums { get; set; } = new List<Album>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 20;
    }
    
    public class AlbumDetailViewModel
    {
        public Album Album { get; set; } = new Album();
        public List<Song> Songs { get; set; } = new List<Song>();
        public int TotalDuration { get; set; }
    }
    
    #endregion
    
    #region Artists ViewModels
    
    public class ArtistsViewModel
    {
        public IEnumerable<Artist> Artists { get; set; } = new List<Artist>();
        public string? Search { get; set; }
        public string Sort { get; set; } = "popular";
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }
    
    public class ArtistListViewModel
    {
        public List<Artist> Artists { get; set; } = new List<Artist>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 20;
    }
    
    public class ArtistDetailViewModel
    {
        public Artist Artist { get; set; } = new Artist();
        public List<Song> PopularSongs { get; set; } = new List<Song>();
        public List<Album> Albums { get; set; } = new List<Album>();
        public int TotalSongs { get; set; }
        public int TotalAlbums { get; set; }
    }
    
    #endregion
    
    #region Playlists ViewModels
    
    public class PlaylistsViewModel
    {
        public IEnumerable<Playlist> Playlists { get; set; } = new List<Playlist>();
        public string? Search { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }
    
    public class PlaylistDetailViewModel
    {
        public Playlist Playlist { get; set; } = new Playlist();
        public List<PlaylistSong> Songs { get; set; } = new List<PlaylistSong>();
        public int TotalDuration { get; set; }
        public bool IsOwner { get; set; }
    }
    
    #endregion
    
    #region Profile ViewModels
    
    public class ProfileViewModel
    {
        public ApplicationUser User { get; set; } = new ApplicationUser();
        public List<Playlist> Playlists { get; set; } = new List<Playlist>();
        public int TotalPlaylists { get; set; }
        public int TotalLikedSongs { get; set; }
        public bool IsOwnProfile { get; set; }
    }

    public class EditProfileViewModel
    {
        [StringLength(100)]
        [Display(Name = "Tên hiển thị")]
        public string? DisplayName { get; set; }

        [StringLength(500)]
        [Display(Name = "Giới thiệu")]
        public string? Bio { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(20)]
        [Display(Name = "Giới tính")]
        public string? Gender { get; set; }

        [StringLength(100)]
        [Display(Name = "Quốc gia")]
        public string? Country { get; set; }

        public IFormFile? AvatarFile { get; set; }
    }

    public class CreatePlaylistViewModel
    {
        [Required(ErrorMessage = "Tên playlist là bắt buộc")]
        [StringLength(200)]
        [Display(Name = "Tên playlist")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Công khai")]
        public bool IsPublic { get; set; }

        public IFormFile? CoverFile { get; set; }
    }

    public class EditPlaylistViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên playlist là bắt buộc")]
        [StringLength(200)]
        [Display(Name = "Tên playlist")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Công khai")]
        public bool IsPublic { get; set; }

        public string? CurrentCoverUrl { get; set; }

        public IFormFile? CoverFile { get; set; }
    }

    public class AddToPlaylistViewModel
    {
        public int SongId { get; set; }
        public string? SongTitle { get; set; }
        public List<Playlist> UserPlaylists { get; set; } = new List<Playlist>();
    }
    
    #endregion

    #region Rating & Comment ViewModels

    public class SongRatingViewModel
    {
        public int SongId { get; set; }
        
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }
    }

    public class SongRatingDisplayViewModel
    {
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public int[] RatingDistribution { get; set; } = new int[5]; // Count per star
        public int? UserRating { get; set; } // Current user's rating
    }

    public class SongCommentViewModel
    {
        public int SongId { get; set; }
        
        [Required(ErrorMessage = "Please enter a comment")]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string Content { get; set; } = string.Empty;
    }

    public class SongCommentEditViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Please enter a comment")]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string Content { get; set; } = string.Empty;
    }

    public class SongCommentDisplayViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsEdited { get; set; }
        public bool IsOwner { get; set; } // True if current user owns this comment
    }

    public class SongCommentsListViewModel
    {
        public int SongId { get; set; }
        public List<SongCommentDisplayViewModel> Comments { get; set; } = new();
        public int TotalComments { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    #endregion
}

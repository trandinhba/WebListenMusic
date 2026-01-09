using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebListenMusic.Models
{
    /// <summary>
    /// Database Context chính của ứng dụng MusicListen
    /// Kế thừa từ IdentityDbContext để hỗ trợ xác thực người dùng
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ========================================
        // CÁC BẢNG DỮ LIỆU (DbSets)
        // ========================================
        
        /// <summary>Bảng Bài hát - Lưu thông tin các bài hát</summary>
        public DbSet<Song> Songs { get; set; }
        
        /// <summary>Bảng Album - Lưu thông tin các album nhạc</summary>
        public DbSet<Album> Albums { get; set; }
        
        /// <summary>Bảng Nghệ sĩ - Lưu thông tin các nghệ sĩ/ca sĩ</summary>
        public DbSet<Artist> Artists { get; set; }
        
        /// <summary>Bảng Thể loại - Lưu các thể loại nhạc (Pop, Rock, Ballad...)</summary>
        public DbSet<Genre> Genres { get; set; }
        
        /// <summary>Bảng Playlist - Lưu các danh sách phát của người dùng</summary>
        public DbSet<Playlist> Playlists { get; set; }
        
        /// <summary>Bảng liên kết Playlist-Song - Quan hệ nhiều-nhiều</summary>
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }
        
        /// <summary>Bảng Báo cáo - Lưu các báo cáo vi phạm từ người dùng</summary>
        public DbSet<Report> Reports { get; set; }

        /// <summary>Bảng Bài hát yêu thích - Lưu bài hát được yêu thích của người dùng</summary>
        public DbSet<FavoriteSong> FavoriteSongs { get; set; }

        /// <summary>Bảng Lịch sử nghe - Lưu lịch sử nghe nhạc của người dùng</summary>
        public DbSet<ListeningHistory> ListeningHistories { get; set; }

        /// <summary>Bảng Đánh giá bài hát - Lưu rating của người dùng</summary>
        public DbSet<SongRating> SongRatings { get; set; }

        /// <summary>Bảng Bình luận bài hát - Lưu comment của người dùng</summary>
        public DbSet<SongComment> SongComments { get; set; }

        /// <summary>
        /// Cấu hình quan hệ giữa các bảng và các ràng buộc
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ========================================
            // CẤU HÌNH BẢNG SONG (Bài hát)
            // ========================================
            builder.Entity<Song>(entity =>
            {
                // Tạo index để tăng tốc tìm kiếm
                entity.HasIndex(e => e.Title);      // Index theo tiêu đề
                entity.HasIndex(e => e.IsPublished); // Index theo trạng thái xuất bản
                entity.HasIndex(e => e.IsFeatured);  // Index theo bài hát nổi bật
                entity.HasIndex(e => e.PlayCount);   // Index theo lượt nghe

                // Quan hệ với Artist: Mỗi bài hát thuộc 1 nghệ sĩ
                // OnDelete.SetNull: Khi xóa nghệ sĩ, ArtistId sẽ thành NULL
                entity.HasOne(d => d.Artist)
                    .WithMany(p => p.Songs)
                    .HasForeignKey(d => d.ArtistId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Quan hệ với Album: Mỗi bài hát có thể thuộc 1 album
                entity.HasOne(d => d.Album)
                    .WithMany(p => p.Songs)
                    .HasForeignKey(d => d.AlbumId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Quan hệ với Genre: Mỗi bài hát thuộc 1 thể loại
                entity.HasOne(d => d.Genre)
                    .WithMany(p => p.Songs)
                    .HasForeignKey(d => d.GenreId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ========================================
            // CẤU HÌNH BẢNG ALBUM
            // ========================================
            builder.Entity<Album>(entity =>
            {
                entity.HasIndex(e => e.Title);       // Index theo tên album
                entity.HasIndex(e => e.IsPublished); // Index theo trạng thái

                // Quan hệ với Artist: Mỗi album thuộc 1 nghệ sĩ
                entity.HasOne(d => d.Artist)
                    .WithMany(p => p.Albums)
                    .HasForeignKey(d => d.ArtistId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ========================================
            // CẤU HÌNH BẢNG ARTIST (Nghệ sĩ)
            // ========================================
            builder.Entity<Artist>(entity =>
            {
                entity.HasIndex(e => e.Name); // Index theo tên nghệ sĩ
            });

            // ========================================
            // CẤU HÌNH BẢNG GENRE (Thể loại)
            // ========================================
            builder.Entity<Genre>(entity =>
            {
                // Tên thể loại là duy nhất (không trùng)
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // ========================================
            // CẤU HÌNH BẢNG PLAYLIST
            // ========================================
            builder.Entity<Playlist>(entity =>
            {
                entity.HasIndex(e => e.Name);     // Index theo tên playlist
                entity.HasIndex(e => e.IsPublic); // Index theo trạng thái công khai

                // Quan hệ với User: Mỗi playlist thuộc 1 người dùng
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Playlists)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PlaylistSong configurations
            builder.Entity<PlaylistSong>(entity =>
            {
                entity.HasIndex(e => new { e.PlaylistId, e.SongId }).IsUnique();

                entity.HasOne(d => d.Playlist)
                    .WithMany(p => p.PlaylistSongs)
                    .HasForeignKey(d => d.PlaylistId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Song)
                    .WithMany(p => p.PlaylistSongs)
                    .HasForeignKey(d => d.SongId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Report configurations
            builder.Entity<Report>(entity =>
            {
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Type);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Reports)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.RelatedSong)
                    .WithMany()
                    .HasForeignKey(d => d.RelatedSongId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.RelatedAlbum)
                    .WithMany()
                    .HasForeignKey(d => d.RelatedAlbumId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ========================================
            // CẤU HÌNH BẢNG FAVORITESONG (Bài hát yêu thích)
            // ========================================
            builder.Entity<FavoriteSong>(entity =>
            {
                // Unique constraint: Mỗi user chỉ có thể thích 1 bài hát 1 lần
                entity.HasIndex(e => new { e.UserId, e.SongId }).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.SongId);
                entity.HasIndex(e => e.AddedAt);

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Song)
                    .WithMany()
                    .HasForeignKey(d => d.SongId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================
            // CẤU HÌNH BẢNG LISTENINGHISTORY (Lịch sử nghe)
            // ========================================
            builder.Entity<ListeningHistory>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.SongId);
                entity.HasIndex(e => e.ListenedAt);

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Song)
                    .WithMany()
                    .HasForeignKey(d => d.SongId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================
            // CẤU HÌNH BẢNG SONGRATING (Đánh giá bài hát)
            // ========================================
            builder.Entity<SongRating>(entity =>
            {
                // Mỗi user chỉ được đánh giá 1 lần cho mỗi bài hát
                entity.HasIndex(e => new { e.SongId, e.UserId }).IsUnique();
                entity.HasIndex(e => e.SongId);
                entity.HasIndex(e => e.UserId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SongRatings)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Song)
                    .WithMany(p => p.Ratings)
                    .HasForeignKey(d => d.SongId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================
            // CẤU HÌNH BẢNG SONGCOMMENT (Bình luận bài hát)
            // ========================================
            builder.Entity<SongComment>(entity =>
            {
                entity.HasIndex(e => e.SongId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SongComments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Song)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.SongId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data for Genres
            builder.Entity<Genre>().HasData(
                new Genre { Id = 1, Name = "Pop", Description = "Nhạc Pop", Color = "#ff6b6b" },
                new Genre { Id = 2, Name = "Rock", Description = "Nhạc Rock", Color = "#4ecdc4" },
                new Genre { Id = 3, Name = "Hip Hop", Description = "Nhạc Hip Hop/Rap", Color = "#45b7d1" },
                new Genre { Id = 4, Name = "R&B", Description = "Rhythm and Blues", Color = "#96ceb4" },
                new Genre { Id = 5, Name = "EDM", Description = "Electronic Dance Music", Color = "#dfe6e9" },
                new Genre { Id = 6, Name = "Jazz", Description = "Nhạc Jazz", Color = "#ffeaa7" },
                new Genre { Id = 7, Name = "Classical", Description = "Nhạc cổ điển", Color = "#fd79a8" },
                new Genre { Id = 8, Name = "Country", Description = "Nhạc Country", Color = "#a29bfe" },
                new Genre { Id = 9, Name = "V-Pop", Description = "Nhạc Việt Nam", Color = "#00b894" },
                new Genre { Id = 10, Name = "K-Pop", Description = "Nhạc Hàn Quốc", Color = "#e17055" }
            );
        }
    }
}

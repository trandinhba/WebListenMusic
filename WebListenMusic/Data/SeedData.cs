using Microsoft.AspNetCore.Identity;
using WebListenMusic.Models;

/// <summary>
/// Lớp SeedData - Khởi tạo dữ liệu mặc định cho ứng dụng
/// Bao gồm: Roles, Admin user, Nghệ sĩ, Album, Bài hát mẫu
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Khởi tạo dữ liệu ban đầu khi ứng dụng chạy
    /// Được gọi từ Program.cs
    /// </summary>
    /// <param name="serviceProvider">Service provider để lấy các dịch vụ</param>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        // Lấy các service cần thiết từ DI container
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // ========================================
        // 1. Tạo các Role (Vai trò người dùng)
        // ========================================
        // Admin: Quản trị viên - Quyền cao nhất
        // User: Người dùng thường
        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // ========================================
        // 2. Tạo tài khoản Admin mặc định
        // ========================================
        // Email: admin@musiclisten.com
        // Password: Admin@123
        var adminEmail = "admin@musiclisten.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "Administrator",
                EmailConfirmed = true,  // Xác nhận email sẵn
                IsActive = true,        // Kích hoạt tài khoản
                CreatedAt = DateTime.Now
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                // Gán role Admin cho tài khoản
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // ========================================
        // 3. Tạo dữ liệu mẫu nghệ sĩ
        // ========================================
        // Chỉ tạo nếu chưa có nghệ sĩ nào trong database
        if (!context.Artists.Any())
        {
            var artists = new List<Artist>
            {
                new Artist 
                { 
                    Name = "Sơn Tùng M-TP", 
                    Bio = "Ca sĩ, nhạc sĩ nổi tiếng Việt Nam",
                    Country = "Việt Nam",
                    IsVerified = true,
                    FollowersCount = 1000000,
                    ImageUrl = "/images/default-artist.svg"
                },
                new Artist 
                { 
                    Name = "Hoàng Thùy Linh", 
                    Bio = "Ca sĩ, diễn viên Việt Nam",
                    Country = "Việt Nam",
                    IsVerified = true,
                    FollowersCount = 500000,
                    ImageUrl = "/images/default-artist.svg"
                },
                new Artist 
                { 
                    Name = "Đen Vâu", 
                    Bio = "Rapper nổi tiếng Việt Nam",
                    Country = "Việt Nam",
                    IsVerified = true,
                    FollowersCount = 800000,
                    ImageUrl = "/images/default-artist.svg"
                },
                new Artist 
                { 
                    Name = "BLACKPINK", 
                    Bio = "Nhóm nhạc nữ Hàn Quốc",
                    Country = "Hàn Quốc",
                    IsVerified = true,
                    FollowersCount = 5000000,
                    ImageUrl = "/images/default-artist.svg"
                },
                new Artist 
                { 
                    Name = "BTS", 
                    Bio = "Nhóm nhạc nam Hàn Quốc",
                    Country = "Hàn Quốc",
                    IsVerified = true,
                    FollowersCount = 8000000,
                    ImageUrl = "/images/default-artist.svg"
                }
            };
            context.Artists.AddRange(artists);
            await context.SaveChangesAsync();
        }

        // ========================================
        // 4. Tạo dữ liệu mẫu album
        // ========================================
        // Seed sample albums
        if (!context.Albums.Any())
        {
            var sonTung = context.Artists.FirstOrDefault(a => a.Name == "Sơn Tùng M-TP");
            var denVau = context.Artists.FirstOrDefault(a => a.Name == "Đen Vâu");

            var albums = new List<Album>
            {
                new Album
                {
                    Title = "m-tp M-TP",
                    Description = "Album đầu tay của Sơn Tùng M-TP",
                    ArtistId = sonTung?.Id,
                    ReleaseDate = new DateTime(2017, 1, 1),
                    CoverImageUrl = "/images/default-album.svg",
                    IsPublished = true
                },
                new Album
                {
                    Title = "Đ.V.D",
                    Description = "Album của Đen Vâu",
                    ArtistId = denVau?.Id,
                    ReleaseDate = new DateTime(2020, 1, 1),
                    CoverImageUrl = "/images/default-album.svg",
                    IsPublished = true
                }
            };
            context.Albums.AddRange(albums);
            await context.SaveChangesAsync();
        }

        // Seed sample songs
        if (!context.Songs.Any())
        {
            var sonTung = context.Artists.FirstOrDefault(a => a.Name == "Sơn Tùng M-TP");
            var denVau = context.Artists.FirstOrDefault(a => a.Name == "Đen Vâu");
            var htl = context.Artists.FirstOrDefault(a => a.Name == "Hoàng Thùy Linh");
            var vpop = context.Genres.FirstOrDefault(g => g.Name == "V-Pop");
            var hiphop = context.Genres.FirstOrDefault(g => g.Name == "Hip Hop");
            var album1 = context.Albums.FirstOrDefault(a => a.Title == "m-tp M-TP");

            var songs = new List<Song>
            {
                new Song
                {
                    Title = "Chạy Ngay Đi",
                    ArtistId = sonTung?.Id,
                    GenreId = vpop?.Id,
                    AlbumId = album1?.Id,
                    Duration = 245,
                    PlayCount = 50000000,
                    LikeCount = 1000000,
                    IsPublished = true,
                    IsFeatured = true,
                    ReleaseDate = new DateTime(2018, 5, 1),
                    CoverImageUrl = "/images/default-song.svg",
                    AudioUrl = "/uploads/songs/sample.mp3"
                },
                new Song
                {
                    Title = "Lạc Trôi",
                    ArtistId = sonTung?.Id,
                    GenreId = vpop?.Id,
                    Duration = 276,
                    PlayCount = 80000000,
                    LikeCount = 1500000,
                    IsPublished = true,
                    IsFeatured = true,
                    ReleaseDate = new DateTime(2017, 1, 1),
                    CoverImageUrl = "/images/default-song.svg",
                    AudioUrl = "/uploads/songs/sample.mp3"
                },
                new Song
                {
                    Title = "Bài Này Chill Phết",
                    ArtistId = denVau?.Id,
                    GenreId = hiphop?.Id,
                    Duration = 312,
                    PlayCount = 60000000,
                    LikeCount = 1200000,
                    IsPublished = true,
                    IsFeatured = true,
                    ReleaseDate = new DateTime(2019, 6, 1),
                    CoverImageUrl = "/images/default-song.svg",
                    AudioUrl = "/uploads/songs/sample.mp3"
                },
                new Song
                {
                    Title = "See Tình",
                    ArtistId = htl?.Id,
                    GenreId = vpop?.Id,
                    Duration = 198,
                    PlayCount = 100000000,
                    LikeCount = 2000000,
                    IsPublished = true,
                    IsFeatured = true,
                    ReleaseDate = new DateTime(2022, 1, 1),
                    CoverImageUrl = "/images/default-song.svg",
                    AudioUrl = "/uploads/songs/sample.mp3"
                },
                new Song
                {
                    Title = "Nơi Này Có Anh",
                    ArtistId = sonTung?.Id,
                    GenreId = vpop?.Id,
                    Duration = 256,
                    PlayCount = 90000000,
                    LikeCount = 1800000,
                    IsPublished = true,
                    IsFeatured = false,
                    ReleaseDate = new DateTime(2017, 2, 14),
                    CoverImageUrl = "/images/default-song.svg",
                    AudioUrl = "/uploads/songs/sample.mp3"
                }
            };
            context.Songs.AddRange(songs);
            await context.SaveChangesAsync();
        }
    }
}

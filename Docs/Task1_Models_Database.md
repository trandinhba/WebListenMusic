# 📋 Task 1: Setup Models & Database

## 🎯 Mục tiêu
Thiết lập cơ sở dữ liệu và các Models cần thiết cho ứng dụng MusicListen.

## ✅ Các công việc đã hoàn thành

### 1. Cài đặt NuGet Packages
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (8.0.0)
- `Microsoft.EntityFrameworkCore.SqlServer` (8.0.0)
- `Microsoft.EntityFrameworkCore.Tools` (8.0.0)
- `Microsoft.VisualStudio.Web.CodeGeneration.Design` (8.0.0)

### 2. Tạo Entity Models

#### 📁 Models/Genre.cs
- `Id`: int - Primary key
- `Name`: string - Tên thể loại (bắt buộc, max 100 ký tự)
- `Description`: string? - Mô tả
- `Color`: string? - Màu hiển thị
- `CreatedAt`: DateTime - Ngày tạo

#### 📁 Models/Artist.cs
- `Id`: int - Primary key
- `Name`: string - Tên nghệ sĩ (bắt buộc, max 200 ký tự)
- `Bio`: string? - Tiểu sử
- `ImageUrl`: string? - Ảnh đại diện
- `Country`: string? - Quốc gia
- `BirthDate`: DateTime? - Ngày sinh
- `IsVerified`: bool - Đã xác minh
- `FollowersCount`: int - Lượt theo dõi
- Navigation: Songs, Albums

#### 📁 Models/Album.cs
- `Id`: int - Primary key
- `Title`: string - Tên album
- `Description`: string? - Mô tả
- `CoverImageUrl`: string? - Ảnh bìa
- `ReleaseDate`: DateTime? - Ngày phát hành
- `PlayCount`, `LikeCount`: int - Thống kê
- `ArtistId`: int? - Foreign key
- Navigation: Artist, Songs

#### 📁 Models/Song.cs
- `Id`: int - Primary key
- `Title`: string - Tên bài hát
- `AudioUrl`: string? - File nhạc
- `CoverImageUrl`: string? - Ảnh bìa
- `Duration`: int - Thời lượng (giây)
- `Lyrics`: string? - Lời bài hát
- `PlayCount`, `LikeCount`, `DownloadCount`: int - Thống kê
- `IsPublished`, `IsFeatured`: bool - Trạng thái
- `ArtistId`, `AlbumId`, `GenreId`: int? - Foreign keys
- Navigation: Artist, Album, Genre, PlaylistSongs

#### 📁 Models/ApplicationUser.cs (extends IdentityUser)
- `DisplayName`: string? - Tên hiển thị
- `Bio`: string? - Giới thiệu
- `AvatarUrl`: string? - Ảnh đại diện
- `DateOfBirth`: DateTime? - Ngày sinh
- `Gender`, `Country`: string? - Thông tin cá nhân
- `IsPremium`: bool - Tài khoản Premium
- `IsActive`: bool - Trạng thái hoạt động
- Navigation: Playlists, Reports

#### 📁 Models/Playlist.cs
- `Id`: int - Primary key
- `Name`: string - Tên playlist
- `Description`: string? - Mô tả
- `CoverImageUrl`: string? - Ảnh bìa
- `IsPublic`: bool - Công khai
- `UserId`: string - Foreign key
- Navigation: User, PlaylistSongs

#### 📁 Models/PlaylistSong.cs
- `Id`: int - Primary key
- `Order`: int - Thứ tự
- `AddedAt`: DateTime - Ngày thêm
- `PlaylistId`, `SongId`: int - Foreign keys
- Navigation: Playlist, Song

#### 📁 Models/Report.cs
- `Id`: int - Primary key
- `Title`, `Content`: string - Nội dung báo cáo
- `Type`: ReportType enum - Loại báo cáo
- `Status`: ReportStatus enum - Trạng thái
- `AdminNote`: string? - Ghi chú admin
- `UserId`, `ResolvedByUserId`: string - Foreign keys
- `RelatedSongId`, `RelatedAlbumId`: int? - Liên kết

### 3. Tạo DbContext

#### 📁 Data/ApplicationDbContext.cs
- Kế thừa từ `IdentityDbContext<ApplicationUser>`
- Cấu hình DbSet cho tất cả entities
- Cấu hình relationships và indexes
- Seed data cho Genres (10 thể loại nhạc)

### 4. Tạo ViewModels

#### 📁 Models/ViewModels/AccountViewModels.cs
- `LoginViewModel`
- `RegisterViewModel`
- `ForgotPasswordViewModel`
- `ResetPasswordViewModel`
- `ChangePasswordViewModel`

#### 📁 Models/ViewModels/HomeViewModels.cs
- `HomeViewModel`
- `SearchViewModel`
- `SongListViewModel`
- `AlbumListViewModel`
- `ArtistListViewModel`
- `AlbumDetailViewModel`
- `ArtistDetailViewModel`
- `PlaylistDetailViewModel`

#### 📁 Models/ViewModels/AdminViewModels.cs
- `AdminDashboardViewModel`
- `AdminSongViewModel`
- `AdminAlbumViewModel`
- `AdminArtistViewModel`
- `AdminUserViewModel`
- `AdminReportViewModel`
- `AdminSongListViewModel`

#### 📁 Models/ViewModels/ProfileViewModels.cs
- `ProfileViewModel`
- `EditProfileViewModel`
- `CreatePlaylistViewModel`
- `EditPlaylistViewModel`
- `AddToPlaylistViewModel`

### 5. Cấu hình Program.cs
- Thêm DbContext với SQL Server
- Cấu hình Identity với ApplicationUser
- Thiết lập cookie authentication
- Thêm Area routing cho Admin
- Gọi SeedData khi khởi động

### 6. Tạo SeedData

#### 📁 Data/SeedData.cs
- Tạo roles: Admin, User
- Tạo admin user: admin@musiclisten.com / Admin@123
- Seed 5 nghệ sĩ mẫu
- Seed 2 album mẫu
- Seed 5 bài hát mẫu

### 7. Tạo FileHelper

#### 📁 Helpers/FileHelper.cs
- `UploadFileAsync()`: Upload file với validation
- `DeleteFile()`: Xóa file
- `FormatDuration()`: Format thời lượng (m:ss)
- `FormatNumber()`: Format số (1K, 1M, 1B)

### 8. Cấu hình appsettings.json
- Connection string cho SQL Server LocalDB
- File settings (max sizes, allowed extensions)

### 9. Tạo thư mục Uploads
- `/wwwroot/uploads/songs/` - Lưu file nhạc
- `/wwwroot/uploads/covers/` - Lưu ảnh bìa
- `/wwwroot/uploads/avatars/` - Lưu avatar

## 📁 Cấu trúc thư mục sau Task 1
```
WebListenMusic/
├── Data/
│   ├── ApplicationDbContext.cs
│   └── SeedData.cs
├── Helpers/
│   └── FileHelper.cs
├── Models/
│   ├── Album.cs
│   ├── ApplicationUser.cs
│   ├── Artist.cs
│   ├── Genre.cs
│   ├── Playlist.cs
│   ├── PlaylistSong.cs
│   ├── Report.cs
│   ├── Song.cs
│   └── ViewModels/
│       ├── AccountViewModels.cs
│       ├── AdminViewModels.cs
│       ├── HomeViewModels.cs
│       └── ProfileViewModels.cs
└── wwwroot/
    └── uploads/
        ├── songs/
        ├── covers/
        └── avatars/
```

## 🔧 Các lệnh cần chạy

```bash
# Restore packages
dotnet restore

# Tạo migration
dotnet ef migrations add InitialCreate

# Cập nhật database
dotnet ef database update
```

## 📊 Database Schema

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Artists   │────<│    Songs    │>────│   Albums    │
└─────────────┘     └─────────────┘     └─────────────┘
                           │
                           │
                    ┌──────┴──────┐
                    │             │
              ┌─────▼─────┐ ┌─────▼─────┐
              │  Genres   │ │PlaylistSongs│
              └───────────┘ └─────┬─────┘
                                  │
                            ┌─────▼─────┐
                            │ Playlists │
                            └─────┬─────┘
                                  │
┌─────────────┐              ┌────▼────┐
│   Reports   │─────────────>│  Users  │
└─────────────┘              └─────────┘
```

## ✅ Trạng thái: HOÀN THÀNH
- [x] NuGet packages
- [x] Entity models (8 models)
- [x] DbContext với configurations
- [x] ViewModels (15 viewmodels)
- [x] Program.cs configuration
- [x] SeedData
- [x] FileHelper
- [x] Upload directories
- [x] appsettings.json

---
**Ngày hoàn thành:** 31/12/2024
**Task tiếp theo:** Task 2 - Design System & Base Layout

# 📚 MusicListen - Tổng hợp Tài liệu Dự án

## 📋 Mục lục

| Task | Tên | File tài liệu | Trạng thái |
|------|-----|---------------|------------|
| 1 | Models & Database | [Task1_Models_Database.md](Task1_Models_Database.md) | ✅ Hoàn thành |
| 2 | Design & Layout | [Task2_Design_Layout.md](Task2_Design_Layout.md) | ✅ Hoàn thành |
| 3 | Admin Foundation | [Task3_Admin_Foundation.md](Task3_Admin_Foundation.md) | ✅ Hoàn thành |
| 4 | Admin Songs CRUD | [Task4_Admin_Songs_CRUD.md](Task4_Admin_Songs_CRUD.md) | ✅ Hoàn thành |
| 5 | Admin Albums CRUD | [Task5_Admin_Albums_CRUD.md](Task5_Admin_Albums_CRUD.md) | ✅ Hoàn thành |
| 6 | Admin Artists CRUD | [Task6_Admin_Artists_CRUD.md](Task6_Admin_Artists_CRUD.md) | ✅ Hoàn thành |
| 7 | Admin Users & Reports | [Task7_Admin_Users_Reports.md](Task7_Admin_Users_Reports.md) | ✅ Hoàn thành |
| 11 | Search & Profile | [Task11_Search_Profile.md](../Task11_Search_Profile.md) | ✅ Hoàn thành |
| 12 | Music Player | [Task12_Music_Player.md](../Task12_Music_Player.md) | ✅ Hoàn thành |
| 13 | Authentication & Genres | [Task13_Authentication_Genres.md](Task13_Authentication_Genres.md) | ✅ Hoàn thành |

---

## 🗂️ Cấu trúc Dự án

```
WebListenMusic/
├── 📁 Models/                    # Data Models
│   ├── ApplicationUser.cs        # Extended IdentityUser
│   ├── Song.cs                   # Bài hát
│   ├── Album.cs                  # Album
│   ├── Artist.cs                 # Nghệ sĩ
│   ├── Genre.cs                  # Thể loại
│   ├── Playlist.cs               # Playlist
│   ├── PlaylistSong.cs           # Bài hát trong playlist
│   ├── Report.cs                 # Báo cáo
│   └── ViewModels/               # View Models
│       ├── AccountViewModels.cs
│       ├── AdminViewModels.cs
│       └── CustomerViewModels.cs
│
├── 📁 Areas/Admin/               # Admin Area
│   ├── Controllers/
│   │   ├── DashboardController.cs
│   │   ├── SongsController.cs
│   │   ├── AlbumsController.cs
│   │   ├── ArtistsController.cs
│   │   ├── GenresController.cs   # ✨ Mới thêm
│   │   ├── UsersController.cs
│   │   └── ReportsController.cs
│   └── Views/
│       ├── Dashboard/
│       ├── Songs/
│       ├── Albums/
│       ├── Artists/
│       ├── Genres/               # ✨ Mới thêm
│       ├── Users/
│       ├── Reports/
│       └── Shared/_AdminLayout.cshtml
│
├── 📁 Controllers/               # Customer Controllers
│   ├── AccountController.cs      # ✨ Mới thêm
│   ├── HomeController.cs
│   ├── SongsController.cs
│   ├── AlbumsController.cs
│   ├── ArtistsController.cs
│   ├── PlaylistsController.cs
│   ├── ProfileController.cs
│   └── SearchController.cs
│
├── 📁 Views/                     # Customer Views
│   ├── Account/                  # ✨ Mới thêm
│   │   ├── Login.cshtml
│   │   ├── Register.cshtml
│   │   ├── AccessDenied.cshtml
│   │   ├── Lockout.cshtml
│   │   └── ChangePassword.cshtml
│   ├── Home/
│   ├── Songs/
│   ├── Albums/
│   ├── Artists/
│   ├── Playlists/
│   ├── Profile/
│   ├── Search/
│   └── Shared/
│       ├── _Layout.cshtml
│       ├── _Player.cshtml
│       └── _AddToPlaylistModal.cshtml
│
├── 📁 Data/
│   ├── ApplicationDbContext.cs
│   └── SeedData.cs
│
├── 📁 wwwroot/
│   ├── css/
│   │   ├── theme.css             # Design system
│   │   ├── components.css        # UI components
│   │   ├── pages.css             # Page-specific
│   │   └── admin.css             # Admin styles
│   ├── js/
│   │   ├── site.js               # Common JS
│   │   ├── player.js             # Music player
│   │   └── admin.js              # Admin JS
│   └── uploads/
│       ├── songs/
│       ├── covers/
│       └── avatars/
│
└── 📁 Docs/                      # Tài liệu
    ├── Task1_Models_Database.md
    ├── Task2_Design_Layout.md
    ├── ...
    └── PROJECT_OVERVIEW.md       # File này
```

---

## 🎨 Design System

### Color Palette
```css
--ml-bg: #0b1220;           /* Background chính */
--ml-surface: #111a2e;       /* Card, modal */
--ml-primary: #1f6feb;       /* Buttons, links */
--ml-success: #22c55e;       /* Success states */
--ml-danger: #ef4444;        /* Errors */
--ml-warning: #f59e0b;       /* Warnings */
--ml-text: #e6edf3;          /* Text chính */
--ml-text-muted: #9aa7b3;    /* Text phụ */
```

### Typography
- Font: Inter (Google Fonts)
- Icons: Bootstrap Icons

### Spacing
```css
--ml-sidebar-width: 240px;
--ml-player-height: 90px;
--ml-header-height: 64px;
```

---

## 🔐 Roles & Permissions

| Role | Quyền |
|------|-------|
| **Admin** | Full access, quản lý toàn bộ hệ thống |
| **User** | Nghe nhạc, tạo playlist, quản lý profile |

---

## 🔗 Route Map

### Public Routes
| Route | Controller | Mô tả |
|-------|------------|-------|
| `/` | Home | Trang chủ |
| `/Songs` | Songs | Danh sách bài hát |
| `/Albums` | Albums | Danh sách album |
| `/Artists` | Artists | Danh sách nghệ sĩ |
| `/Search` | Search | Tìm kiếm |

### Authentication Routes
| Route | Controller | Mô tả |
|-------|------------|-------|
| `/Account/Login` | Account | Đăng nhập |
| `/Account/Register` | Account | Đăng ký |
| `/Account/Logout` | Account | Đăng xuất |

### User Routes (Require Login)
| Route | Controller | Mô tả |
|-------|------------|-------|
| `/Profile` | Profile | Hồ sơ cá nhân |
| `/Playlists` | Playlists | Quản lý playlist |

### Admin Routes (Require Admin Role)
| Route | Controller | Mô tả |
|-------|------------|-------|
| `/Admin` | Dashboard | Dashboard |
| `/Admin/Songs` | Songs | Quản lý bài hát |
| `/Admin/Albums` | Albums | Quản lý album |
| `/Admin/Artists` | Artists | Quản lý nghệ sĩ |
| `/Admin/Genres` | Genres | Quản lý thể loại |
| `/Admin/Users` | Users | Quản lý người dùng |
| `/Admin/Reports` | Reports | Quản lý báo cáo |

---

## 🎵 Features Checklist

### ✅ Hoàn thành

#### Admin
- [x] Dashboard với thống kê
- [x] CRUD Bài hát (upload audio + cover)
- [x] CRUD Album
- [x] CRUD Nghệ sĩ
- [x] CRUD Thể loại
- [x] Quản lý Users
- [x] Quản lý Reports

#### Customer
- [x] Trang chủ (Trending, New, Top Artists)
- [x] Duyệt bài hát với filter
- [x] Duyệt album
- [x] Duyệt nghệ sĩ
- [x] CRUD Playlist
- [x] Profile (View, Edit, Settings)
- [x] Tìm kiếm đa loại

#### Authentication
- [x] Đăng ký
- [x] Đăng nhập
- [x] Đăng xuất
- [x] Đổi mật khẩu
- [x] Access Denied page
- [x] Lockout page

#### Music Player
- [x] Play/Pause
- [x] Next/Previous
- [x] Progress bar (seekable)
- [x] Volume control
- [x] Shuffle & Repeat
- [x] Queue management

### 🔄 Có thể mở rộng

- [ ] Email confirmation
- [ ] Forgot password
- [ ] External login (Google, Facebook)
- [ ] Favorites/Liked songs
- [ ] Listening history
- [ ] Drag & drop reorder playlist
- [ ] Lyrics sync
- [ ] Download offline

---

## 🚀 Hướng dẫn Chạy Dự án

### 1. Yêu cầu
- .NET 8.0 SDK
- SQL Server
- Visual Studio 2022 / VS Code

### 2. Cài đặt
```bash
# Clone project
git clone <repository-url>
cd WebListenMusic

# Restore packages
dotnet restore

# Update database
dotnet ef database update

# Run
dotnet run
```

### 3. Tài khoản mặc định
```
Admin: admin@musiclisten.com / Admin@123
User:  user@musiclisten.com / User@123
```

---

## 📝 Ghi chú Bảo trì

### Thêm Model mới
1. Tạo file trong `Models/`
2. Thêm DbSet trong `ApplicationDbContext.cs`
3. Tạo migration: `dotnet ef migrations add <Name>`
4. Update database: `dotnet ef database update`

### Thêm Controller mới (Admin)
1. Tạo trong `Areas/Admin/Controllers/`
2. Thêm `[Area("Admin")]` và `[Authorize(Roles = "Admin")]`
3. Tạo Views tương ứng

### Thêm CSS
- Global: `wwwroot/css/theme.css`
- Components: `wwwroot/css/components.css`
- Pages: `wwwroot/css/pages.css`
- Admin only: `wwwroot/css/admin.css`

---

## 📞 Liên hệ

- **Author:** MusicListen Team
- **Version:** 1.0.0
- **Last Updated:** December 2025

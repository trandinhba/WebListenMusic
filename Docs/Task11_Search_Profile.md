# Task 11: Search & Profile - Chi tiết triển khai

## 📋 Tổng quan

Task 11 triển khai chức năng tìm kiếm đa loại và quản lý hồ sơ người dùng, bao gồm xem/chỉnh sửa profile, cài đặt tài khoản, và các trang yêu thích/lịch sử.

## 📁 Các file được tạo/cập nhật

### Controllers

#### 1. SearchController.cs
**Đường dẫn:** `Controllers/SearchController.cs`

**Chức năng chính:**
- **Index**: Tìm kiếm đa loại với filter theo type (all/songs/albums/artists/playlists)
- **Quick**: API endpoint cho autocomplete/gợi ý tìm kiếm

**Code mẫu:**
```csharp
public async Task<IActionResult> Index(string? q, string type = "all")
{
    var viewModel = new SearchViewModel { Query = q, Type = type };
    
    if (string.IsNullOrEmpty(q)) return View(viewModel);

    // Tìm kiếm theo từng loại
    if (type == "all" || type == "songs")
    {
        viewModel.Songs = await _context.Songs
            .Where(s => s.Title.Contains(q) || s.Artist.Name.Contains(q))
            .Take(6).ToListAsync();
    }
    // ... tương tự cho albums, artists, playlists
}
```

#### 2. ProfileController.cs
**Đường dẫn:** `Controllers/ProfileController.cs`

**Actions:**
- **Index(string? userId)**: Xem hồ sơ (của mình hoặc người khác)
- **Edit**: Chỉnh sửa hồ sơ với upload avatar
- **Settings**: Trang cài đặt tài khoản
- **Favorites**: Danh sách bài hát yêu thích
- **History**: Lịch sử nghe nhạc

**Tính năng đặc biệt:**
- Hỗ trợ xem profile người khác qua userId parameter
- Upload và lưu avatar với FileHelper
- Phân biệt IsOwnProfile để hiển thị UI phù hợp

### ViewModels

#### SearchProfileViewModel.cs
**Đường dẫn:** `Models/ViewModels/SearchProfileViewModel.cs`

```csharp
public class SearchViewModel
{
    public string? Query { get; set; }
    public string Type { get; set; } = "all";
    public List<Song> Songs { get; set; } = new();
    public List<Album> Albums { get; set; } = new();
    public List<Artist> Artists { get; set; } = new();
    public List<Playlist> Playlists { get; set; } = new();
    
    public bool HasResults => Songs.Any() || Albums.Any() || 
                              Artists.Any() || Playlists.Any();
}

public class ProfileViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public bool IsOwnProfile { get; set; }
    public List<Playlist> Playlists { get; set; } = new();
    public int TotalPlaylists { get; set; }
}
```

### Views

#### Search Views

##### Views/Search/Index.cshtml
**Tính năng:**
- Hero section với search input lớn
- Tab filter (Tất cả / Bài hát / Album / Nghệ sĩ / Playlist)
- Hiển thị kết quả theo từng loại với "Xem tất cả"
- Empty state khi không có kết quả
- Gợi ý tìm kiếm khi chưa nhập keyword

**Layout:**
```
┌─────────────────────────────────────────┐
│           TÌM KIẾM                      │
│  ┌────────────────────────┐ [Tìm kiếm]  │
│  │ 🔍 Nhập từ khóa...     │             │
│  └────────────────────────┘             │
├─────────────────────────────────────────┤
│ [Tất cả] [Bài hát] [Album] [Nghệ sĩ]   │
├─────────────────────────────────────────┤
│ Bài hát                    [Xem tất cả] │
│ ┌─┬─┬─┬─┬─┬─┐                          │
│ │1│2│3│4│5│6│ (Song list items)        │
│ └─┴─┴─┴─┴─┴─┘                          │
├─────────────────────────────────────────┤
│ Nghệ sĩ                    [Xem tất cả] │
│ ○ ○ ○ ○  (Artist circles)              │
├─────────────────────────────────────────┤
│ Album                      [Xem tất cả] │
│ ▢ ▢ ▢ ▢  (Album grid)                  │
└─────────────────────────────────────────┘
```

#### Profile Views

##### Views/Profile/Index.cshtml
**Tính năng:**
- Profile header với avatar lớn (200px)
- Badge Premium nếu có
- Stats (số playlist, ngày tham gia)
- Quick links (Yêu thích, Lịch sử, Tạo playlist) - chỉ hiện với own profile
- Grid playlist của người dùng
- Responsive design (mobile/desktop)

##### Views/Profile/Edit.cshtml
**Tính năng:**
- Upload avatar với preview realtime
- Form chỉnh sửa DisplayName, Bio
- Email hiển thị readonly

##### Views/Profile/Settings.cshtml
**Tính năng:**
- Quản lý tài khoản (email, mật khẩu)
- Trạng thái Premium với ngày hết hạn
- Cài đặt thông báo (toggles)
- Bảo mật & Quyền riêng tư
- Vùng nguy hiểm (Xóa tài khoản)

##### Views/Profile/Favorites.cshtml & History.cshtml
- Placeholder views với empty state
- Sẵn sàng để implement chức năng chi tiết

## 🎨 CSS Styles

### Search Page Styles
```css
/* Search Hero */
.search-hero { text-align: center; padding: 40px 0; }
.search-form-large { display: flex; max-width: 700px; margin: 0 auto; }
.search-input-wrap { flex: 1; position: relative; }

/* Search Tabs */
.search-tabs { display: flex; gap: 8px; border-bottom: 1px solid var(--ml-border); }
.search-tab { padding: 8px 20px; border-radius: var(--ml-radius-full); }
.search-tab.active { background: var(--ml-primary); color: white; }

/* Suggestion Tags */
.suggestion-tag { padding: 10px 24px; background: var(--ml-surface); }
.suggestion-tag:hover { background: var(--ml-primary); }
```

### Profile Page Styles
```css
/* Profile Header */
.profile-header { display: flex; gap: 32px; align-items: flex-end; }
.profile-avatar { width: 200px; height: 200px; }
.profile-avatar img { border-radius: 50%; box-shadow: var(--ml-shadow-lg); }

/* Premium Badge */
.premium-badge { 
    background: linear-gradient(135deg, #f59e0b 0%, #fbbf24 100%);
    border-radius: 50%;
}

/* Quick Links */
.quick-links { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); }
.quick-link-card { display: flex; align-items: center; gap: 16px; padding: 20px; }
```

## 🔗 API Endpoints

### Search Controller
| Method | Route | Mô tả |
|--------|-------|-------|
| GET | `/Search` | Trang tìm kiếm với kết quả |
| GET | `/Search?q={query}&type={type}` | Tìm kiếm với filter |
| GET | `/Search/Quick?q={query}` | API autocomplete (JSON) |

### Profile Controller
| Method | Route | Mô tả |
|--------|-------|-------|
| GET | `/Profile` | Hồ sơ của mình |
| GET | `/Profile?userId={id}` | Xem hồ sơ người khác |
| GET | `/Profile/Edit` | Form chỉnh sửa |
| POST | `/Profile/Edit` | Lưu thay đổi |
| GET | `/Profile/Settings` | Cài đặt tài khoản |
| GET | `/Profile/Favorites` | Bài hát yêu thích |
| GET | `/Profile/History` | Lịch sử nghe |

## 🔒 Authorization

- **Search**: Public access
- **Profile/Index**: Public (có thể xem profile người khác)
- **Profile/Edit, Settings, Favorites, History**: Yêu cầu đăng nhập

## 📱 Responsive Design

### Search Page
- Desktop: Search form full width, grid 6 columns
- Tablet: Grid 4 columns
- Mobile: Search form stacked, grid 2 columns

### Profile Page
- Desktop: Avatar bên trái, info bên phải
- Mobile: Avatar và info centered, stacked

## 🧪 Test Scenarios

### Search
1. ✅ Tìm kiếm không keyword → Hiện gợi ý
2. ✅ Tìm kiếm có kết quả → Hiện đúng sections
3. ✅ Tìm kiếm không kết quả → Hiện empty state
4. ✅ Filter theo type → Chỉ hiện loại đó
5. ✅ Click "Xem tất cả" → Chuyển sang type cụ thể

### Profile
1. ✅ Xem profile của mình → Hiện quick links, nút Edit
2. ✅ Xem profile người khác → Ẩn quick links, ẩn nút Edit
3. ✅ Edit profile → Upload avatar, lưu thành công
4. ✅ Settings → Các toggle hoạt động
5. ✅ Profile chưa có playlist → Hiện empty state với CTA

## 📦 Dependencies

- Entity Framework Core cho data access
- ASP.NET Core Identity cho user management
- Bootstrap 5 cho responsive grid
- Bootstrap Icons cho icons

## 📈 Improvements có thể thêm

1. **Search autocomplete**: Gợi ý realtime khi gõ
2. **Search history**: Lưu lịch sử tìm kiếm
3. **Advanced filters**: Filter theo genre, year, etc.
4. **Profile social**: Follow/unfollow users
5. **Activity feed**: Hiện hoạt động gần đây
6. **Favorites sync**: Đồng bộ yêu thích realtime
7. **History tracking**: Lưu và hiển thị lịch sử nghe

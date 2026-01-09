# Task 6: Admin Artists CRUD

## Tổng quan
Task này triển khai CRUD cho quản lý Nghệ sĩ với giao diện card-based và trang profile chi tiết đẹp mắt.

## Các file đã tạo

### 1. Controller

#### `Areas/Admin/Controllers/ArtistsController.cs`

**Actions:**

| Action | HTTP | Mô tả |
|--------|------|-------|
| Index | GET | Danh sách nghệ sĩ dạng grid |
| Details | GET | Trang profile nghệ sĩ |
| Create | GET/POST | Thêm nghệ sĩ mới |
| Edit | GET/POST | Chỉnh sửa nghệ sĩ |
| Delete | POST | Xóa nghệ sĩ |
| ToggleStatus | POST | Bật/tắt trạng thái |
| ToggleVerified | POST | Bật/tắt xác thực |

**Index Features:**
- PageSize = 12
- Filter theo search (name)
- Filter theo isVerified (đã xác thực/chưa)
- Include Songs và Albums để đếm và tính plays

**Details Features:**
- Lấy Top 10 songs theo PlayCount
- Lấy 5 albums mới nhất
- Tính tổng thống kê (songs, plays, likes, albums)

### 2. Views

#### `Areas/Admin/Views/Artists/Index.cshtml`

**Grid Layout:**
- 200px minimum card width
- Avatar tròn với overlay actions
- Badge verified
- Thống kê: số bài, số album, lượt nghe

**Artist Card Components:**
- `.artist-card-admin`: Card container
- `.artist-avatar`: Avatar tròn 120px
- `.artist-overlay`: Action buttons overlay
- `.verified-badge`: Badge xác thực (check icon)

#### `Areas/Admin/Views/Artists/Create.cshtml`

**Form Layout:**
- Main: Name, Country, Bio, Cover Image upload
- Sidebar: IsActive, IsVerified switches, Avatar upload

**File Uploads:**
- Avatar: 500x500px
- Cover: 1500x500px

#### `Areas/Admin/Views/Artists/Edit.cshtml`

Tương tự Create với preview images hiện tại.

#### `Areas/Admin/Views/Artists/Details.cshtml`

**Profile Header (Hero Section):**
- Full-width cover image với gradient overlay
- Large avatar (160px)
- Artist name với verified badge
- Stats: songs, albums, plays, likes
- Country info

**Content Sections:**
- Tiểu sử (nếu có)
- Top 10 bài hát phổ biến (table)
- Albums (mini grid cards)

**Sidebar:**
- Trạng thái (active/inactive)
- Xác thực (verified/unverified)
- Follower count
- Ngày tạo/cập nhật

## ViewModel

```csharp
public class AdminArtistViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Tên nghệ sĩ là bắt buộc")]
    [StringLength(200)]
    public string Name { get; set; }
    
    public string? Bio { get; set; }
    public string? Country { get; set; }
    
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; } = true;
    
    public string? ImageUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    
    public IFormFile? Image { get; set; }
    public IFormFile? CoverImage { get; set; }
    
    // Statistics
    public int SongCount { get; set; }
    public int AlbumCount { get; set; }
    public int TotalPlays { get; set; }
    public int FollowerCount { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
```

## CSS Highlights

### Profile Header
```css
.artist-profile-header {
    height: 300px;
    background-size: cover;
    background-position: center;
}

.artist-profile-overlay {
    background: linear-gradient(transparent 0%, rgba(0, 0, 0, 0.8) 100%);
}

.artist-profile-avatar {
    width: 160px;
    height: 160px;
    border-radius: 50%;
    border: 4px solid white;
}
```

### Verified Badge
```css
.verified-badge {
    position: absolute;
    bottom: 4px;
    right: 4px;
    width: 28px;
    height: 28px;
    background: var(--ml-primary);
    border-radius: 50%;
    color: white;
}
```

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| /Admin/Artists | GET | List artists |
| /Admin/Artists/Details/{id} | GET | Artist profile |
| /Admin/Artists/Create | GET/POST | Create artist |
| /Admin/Artists/Edit/{id} | GET/POST | Edit artist |
| /Admin/Artists/Delete/{id} | POST | Delete artist |
| /Admin/Artists/ToggleStatus/{id} | POST | Toggle active |
| /Admin/Artists/ToggleVerified/{id} | POST | Toggle verified |

## Business Rules

1. **Delete Validation**: Không xóa nghệ sĩ có songs hoặc albums
2. **Verified Status**: Toggle qua AJAX, không reload page
3. **File Storage**:
   - Avatars: `/uploads/artists/`
   - Covers: `/uploads/artists/covers/`

## Features

### Verified Badge
- Hiển thị icon check xanh trên avatar
- Label "Nghệ sĩ được xác thực" trên profile
- Filter trong danh sách

### Statistics Display
- Tổng songs, albums
- Tổng plays từ tất cả songs
- Tổng likes
- Follower count

## Tasks tiếp theo

- **Task 7**: Admin Users & Reports
- **Task 8**: Customer Home Page

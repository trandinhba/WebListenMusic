# Task 5: Admin Albums CRUD

## Tổng quan
Task này triển khai đầy đủ CRUD cho quản lý Album trong Admin Area, hiển thị dạng grid cards và tích hợp với Songs.

## Các file đã tạo

### 1. Controller

#### `Areas/Admin/Controllers/AlbumsController.cs`

**Actions:**

| Action | HTTP | Mô tả |
|--------|------|-------|
| Index | GET | Danh sách albums dạng grid với filters |
| Details | GET | Chi tiết album + danh sách bài hát |
| Create | GET | Form thêm album mới |
| Create | POST | Xử lý thêm album |
| Edit | GET | Form chỉnh sửa album |
| Edit | POST | Xử lý cập nhật album |
| Delete | POST | Xóa album (kiểm tra không có bài hát) |
| ToggleStatus | POST | Bật/tắt trạng thái |

**Index Features:**
- PageSize = 12 (grid layout)
- Filter theo search (title, artist name)
- Filter theo artistId
- Filter theo year (năm phát hành)
- Include Songs để đếm số lượng và tổng plays

**Delete Validation:**
- Kiểm tra album có bài hát không
- Không cho xóa nếu có bài hát liên quan
- Hiển thị thông báo lỗi

### 2. Views

#### `Areas/Admin/Views/Albums/Index.cshtml`

**Layout dạng Grid:**
- CSS Grid: `repeat(auto-fill, minmax(220px, 1fr))`
- Album cards với hover effects
- Cover image với overlay actions
- Badge trạng thái (active/inactive)

**Album Card Components:**
- `.album-card-admin`: Container
- `.album-cover`: Ảnh bìa với aspect-ratio 1:1
- `.album-overlay`: Overlay với action buttons (View, Edit, Delete)
- `.album-badge`: Badge trạng thái
- `.album-info`: Thông tin (title, artist, meta, stats)

**Filters:**
- Search box
- Artist dropdown
- Year dropdown
- Clear filter button

#### `Areas/Admin/Views/Albums/Create.cshtml`

**Layout 2 cột:**
- Main (8 cols): Title, Artist dropdown, ReleaseYear, Description
- Sidebar (4 cols): IsActive switch, Cover upload, Actions

#### `Areas/Admin/Views/Albums/Edit.cshtml`

Tương tự Create với:
- Hiển thị cover image hiện tại
- Hidden fields cho Id, CoverImageUrl

#### `Areas/Admin/Views/Albums/Details.cshtml`

**Album Header:**
- Large cover image (220x220)
- Album type badge
- Title, Artist link, Year, Song count, Total duration
- Description

**Songs List:**
- Table hiển thị các bài hát trong album
- Columns: #, Bài hát, Thể loại, Thời lượng, Lượt nghe, Trạng thái, Thao tác
- Link "Thêm bài hát" với albumId pre-selected

**Sidebar Stats:**
- Tổng lượt nghe
- Tổng lượt thích
- Số bài hát
- Tổng thời lượng
- Trạng thái
- Ngày tạo/cập nhật

## ViewModel

```csharp
public class AdminAlbumViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Tên album là bắt buộc")]
    [StringLength(200)]
    public string Title { get; set; }
    
    [Required(ErrorMessage = "Vui lòng chọn nghệ sĩ")]
    public int ArtistId { get; set; }
    public string? ArtistName { get; set; }
    
    public int? ReleaseYear { get; set; }
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public string? CoverImageUrl { get; set; }
    public IFormFile? CoverImage { get; set; }
    
    // Statistics
    public int SongCount { get; set; }
    public int TotalPlays { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
```

## CSS Styles (trong Index.cshtml)

```css
.albums-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
    gap: 24px;
}

.album-card-admin {
    /* Card container */
    background: var(--ml-surface);
    border-radius: var(--ml-radius-lg);
    transition: transform, box-shadow;
}

.album-card-admin:hover {
    transform: translateY(-4px);
    box-shadow: var(--ml-shadow-lg);
}

.album-overlay {
    /* Action buttons overlay */
    position: absolute;
    opacity: 0;
    transition: opacity;
}

.album-card-admin:hover .album-overlay {
    opacity: 1;
}
```

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| /Admin/Albums | GET | List albums |
| /Admin/Albums/Details/{id} | GET | Album details + songs |
| /Admin/Albums/Create | GET/POST | Create album |
| /Admin/Albums/Edit/{id} | GET/POST | Edit album |
| /Admin/Albums/Delete/{id} | POST | Delete album |
| /Admin/Albums/ToggleStatus/{id} | POST | Toggle status |

## Business Rules

1. **Delete Validation**: Album không thể xóa nếu có bài hát liên quan
2. **File Storage**: Cover images lưu tại `/uploads/albums/`
3. **Auto Cleanup**: Xóa file cũ khi update cover
4. **Year Filter**: Chỉ hiển thị các năm có trong database

## Tích hợp với Songs

- Details view hiển thị danh sách songs trong album
- Link "Thêm bài hát" với albumId parameter
- Hiển thị tổng thống kê từ songs (plays, likes, duration)

## Tasks tiếp theo

- **Task 6**: Admin Artists CRUD
- **Task 7**: Admin Users & Reports

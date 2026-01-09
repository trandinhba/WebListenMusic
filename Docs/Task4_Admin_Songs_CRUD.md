# Task 4: Admin Songs CRUD

## Tổng quan
Task này triển khai đầy đủ CRUD (Create, Read, Update, Delete) cho quản lý bài hát trong Admin Area, bao gồm upload file âm thanh và ảnh bìa.

## Các file đã tạo

### 1. Controller

#### `Areas/Admin/Controllers/SongsController.cs`

**Attributes:**
```csharp
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SongsController : Controller
```

**Dependencies:**
- `ApplicationDbContext`: Database context
- `IWebHostEnvironment`: Để lấy WebRootPath cho upload files

**Actions:**

| Action | HTTP | Mô tả |
|--------|------|-------|
| Index | GET | Danh sách bài hát với filter & pagination |
| Details | GET | Chi tiết một bài hát |
| Create | GET | Form thêm bài hát mới |
| Create | POST | Xử lý thêm bài hát |
| Edit | GET | Form chỉnh sửa bài hát |
| Edit | POST | Xử lý cập nhật bài hát |
| Delete | POST | Xóa bài hát |
| ToggleStatus | POST | Bật/tắt trạng thái hoạt động |
| BulkDelete | POST | Xóa nhiều bài hát |
| GetAlbumsByArtist | GET | API lấy danh sách album theo nghệ sĩ |

**Index Action Features:**
- Pagination với pageSize = 15
- Filter theo search (title, artist name)
- Filter theo genreId
- Filter theo artistId
- OrderBy CreatedAt descending

**File Upload Handling:**
- Sử dụng `FileHelper.SaveFileAsync()` để upload
- Tự động xóa file cũ khi update
- Lưu vào `wwwroot/uploads/covers` (ảnh) và `wwwroot/uploads/songs` (audio)

### 2. Views

#### `Areas/Admin/Views/Songs/Index.cshtml`

**Features:**
- Danh sách dạng bảng với các cột: #, Bài hát, Nghệ sĩ, Album, Thể loại, Thời lượng, Lượt nghe, Trạng thái, Thao tác
- Search box với live filtering
- Dropdown filter: Thể loại, Nghệ sĩ
- Clear filter button
- Select all checkbox
- Bulk delete với confirmation
- Toggle status bằng AJAX
- Pagination

**UI Components:**
- `admin-table-container`: Container cho bảng
- `admin-table-header`: Header với title và actions
- `admin-table`: Styled table
- `admin-table-footer`: Pagination
- `bulk-actions`: Toolbar cho bulk operations
- `status-badge`: Badge hiển thị trạng thái
- `action-buttons`: Nhóm nút Edit, View, Delete

**JavaScript:**
- `toggleStatus(id, button)`: AJAX call để toggle status
- `confirmBulkDelete()`: Xác nhận xóa hàng loạt

#### `Areas/Admin/Views/Songs/Create.cshtml`

**Layout:**
- 2 cột: Main content (8 cols) + Sidebar (4 cols)

**Main Content:**
- Card: Thông tin bài hát
  - Title input
  - Artist dropdown (với cascade đến Albums)
  - Album dropdown (load động theo Artist)
  - Genre dropdown
  - Duration input
  - Lyrics textarea

- Card: File âm thanh
  - Drag & drop upload area
  - File preview sau khi chọn
  - Hỗ trợ MP3, WAV

**Sidebar:**
- Card: Tùy chọn
  - IsActive switch
  - IsPremium switch

- Card: Ảnh bìa
  - Drag & drop upload
  - Image preview

- Card: Actions
  - Submit button
  - Cancel button

**JavaScript:**
- Cascade dropdown: Khi chọn Artist, load Albums tương ứng

#### `Areas/Admin/Views/Songs/Edit.cshtml`

**Tương tự Create với các điểm khác:**
- Hiển thị file hiện tại (audio player cho file nhạc)
- Hiển thị ảnh bìa hiện tại
- Hidden fields cho Id, CoverImageUrl, AudioUrl
- "Chọn file mới để thay thế" label

#### `Areas/Admin/Views/Songs/Details.cshtml`

**Layout:**
- 2 cột: Main content + Sidebar

**Main Content:**
- Song detail header: Cover image + Title, Artist, Album, Genre
- Audio player section: HTML5 audio player
- Lyrics section: Formatted lyrics display

**Sidebar:**
- Card: Thống kê
  - Lượt nghe
  - Lượt thích
  - Thời lượng

- Card: Trạng thái
  - Trạng thái (Active/Inactive)
  - Premium (Có/Không)

- Card: Thông tin khác
  - Ngày tạo
  - Ngày cập nhật

**Action Buttons:**
- Edit button
- Delete button với confirm

## Luồng dữ liệu

### Create Flow
```
1. User truy cập /Admin/Songs/Create
2. Controller load dropdowns (Artists, Albums, Genres)
3. User điền form và upload files
4. POST submit:
   - Validate form
   - Upload cover image → /uploads/covers/
   - Upload audio file → /uploads/songs/
   - Create Song entity
   - Save to database
   - Redirect to Index với success message
```

### Edit Flow
```
1. User truy cập /Admin/Songs/Edit/{id}
2. Controller load Song và dropdowns
3. View hiển thị data hiện tại
4. User sửa và/hoặc upload file mới
5. POST submit:
   - Validate form
   - Nếu có file mới: Xóa file cũ, upload file mới
   - Update Song entity
   - Save changes
   - Redirect với success message
```

### Delete Flow
```
1. User click Delete button
2. Confirm dialog
3. POST to Delete action:
   - Xóa cover image file
   - Xóa audio file
   - Remove Song từ database
   - Redirect với success message
```

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| /Admin/Songs | GET | List songs |
| /Admin/Songs/Details/{id} | GET | Song details |
| /Admin/Songs/Create | GET | Create form |
| /Admin/Songs/Create | POST | Create song |
| /Admin/Songs/Edit/{id} | GET | Edit form |
| /Admin/Songs/Edit/{id} | POST | Update song |
| /Admin/Songs/Delete/{id} | POST | Delete song |
| /Admin/Songs/ToggleStatus/{id} | POST | Toggle active status |
| /Admin/Songs/BulkDelete | POST | Delete multiple songs |
| /Admin/Songs/GetAlbumsByArtist | GET | Get albums for dropdown |

## ViewModel

```csharp
public class AdminSongViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Tên bài hát là bắt buộc")]
    [StringLength(200)]
    public string Title { get; set; }
    
    [Required(ErrorMessage = "Vui lòng chọn nghệ sĩ")]
    public int ArtistId { get; set; }
    public string? ArtistName { get; set; }
    
    public int? AlbumId { get; set; }
    public string? AlbumTitle { get; set; }
    
    public int? GenreId { get; set; }
    public string? GenreName { get; set; }
    
    public int Duration { get; set; }
    public string? Lyrics { get; set; }
    
    public bool IsActive { get; set; } = true;
    public bool IsPremium { get; set; }
    
    public int PlayCount { get; set; }
    public int LikeCount { get; set; }
    
    public string? CoverImageUrl { get; set; }
    public string? AudioUrl { get; set; }
    
    public IFormFile? CoverImage { get; set; }
    public IFormFile? AudioFile { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
```

## Validation

**Server-side:**
- Title: Required, MaxLength 200
- ArtistId: Required

**Client-side:**
- jQuery Validation
- _ValidationScriptsPartial

## File Upload

**Allowed formats:**
- Images: JPG, PNG, GIF, WebP
- Audio: MP3, WAV, OGG, M4A

**Storage paths:**
- Cover images: `/uploads/covers/{guid}.{ext}`
- Audio files: `/uploads/songs/{guid}.{ext}`

**Max file sizes:**
- Images: 5MB
- Audio: 50MB

## Security

- `[Authorize(Roles = "Admin")]` trên controller
- `@Html.AntiForgeryToken()` cho tất cả forms
- Validation RequestVerificationToken cho AJAX calls
- File type validation
- File size validation

## Tasks tiếp theo

- **Task 5**: Admin Albums CRUD
- **Task 6**: Admin Artists CRUD
- **Task 7**: Admin Users & Reports

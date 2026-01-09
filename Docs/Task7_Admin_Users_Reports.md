# Task 7: Admin Users & Reports Management

## Tổng quan
Task này hoàn thành phần quản lý người dùng (Users) và báo cáo (Reports) trong Admin Area, bao gồm các chức năng xem, lọc, cập nhật trạng thái và xóa.

## Thời gian thực hiện
- **Bắt đầu**: Tiếp nối Task 6
- **Hoàn thành**: Sau Task 6

---

## Backend (BE)

### 1. UsersController.cs
**Đường dẫn**: `Areas/Admin/Controllers/UsersController.cs`

#### Chức năng:
| Action | HTTP Method | Mô tả |
|--------|-------------|-------|
| `Index` | GET | Danh sách người dùng với phân trang |
| `Details` | GET | Chi tiết người dùng |
| `ToggleStatus` | POST | Bật/tắt trạng thái hoạt động |
| `TogglePremium` | POST | Bật/tắt Premium với ngày hết hạn |
| `Delete` | POST | Xóa người dùng (trừ Admin) |

#### Code highlights:

```csharp
// Lấy danh sách người dùng
public async Task<IActionResult> Index(string search, int page = 1)
{
    var query = _userManager.Users.AsQueryable();
    
    if (!string.IsNullOrEmpty(search))
    {
        query = query.Where(u => u.DisplayName.Contains(search) || u.Email.Contains(search));
    }
    
    var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    return View(users);
}
```

```csharp
// Toggle Premium với ngày hết hạn
[HttpPost]
public async Task<IActionResult> TogglePremium(string id)
{
    var user = await _userManager.FindByIdAsync(id);
    user.IsPremium = !user.IsPremium;
    
    if (user.IsPremium)
        user.PremiumExpiresAt = DateTime.Now.AddMonths(1);
    else
        user.PremiumExpiresAt = null;
    
    await _userManager.UpdateAsync(user);
    return Json(new { success = true, isPremium = user.IsPremium, expiresAt = user.PremiumExpiresAt });
}
```

```csharp
// Không cho phép xóa Admin
[HttpPost]
public async Task<IActionResult> Delete(string id)
{
    var user = await _userManager.FindByIdAsync(id);
    var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
    
    if (isAdmin)
        return Json(new { success = false, message = "Không thể xóa tài khoản Admin" });
    
    await _userManager.DeleteAsync(user);
    return RedirectToAction(nameof(Index));
}
```

---

### 2. ReportsController.cs
**Đường dẫn**: `Areas/Admin/Controllers/ReportsController.cs`

#### Chức năng:
| Action | HTTP Method | Mô tả |
|--------|-------------|-------|
| `Index` | GET | Danh sách báo cáo với lọc theo loại & trạng thái |
| `Details` | GET | Chi tiết báo cáo + đối tượng liên quan |
| `UpdateStatus` | POST | Cập nhật trạng thái + ghi chú admin |
| `Delete` | POST | Xóa báo cáo |

#### Code highlights:

```csharp
// Lấy thông tin đối tượng báo cáo
private async Task<object> GetTargetInfo(Report report)
{
    return report.Type switch
    {
        ReportType.Song => await _context.Songs.Include(s => s.Artist)
            .FirstOrDefaultAsync(s => s.Id == report.TargetId),
        ReportType.Album => await _context.Albums.Include(a => a.Artist)
            .FirstOrDefaultAsync(a => a.Id == report.TargetId),
        ReportType.Artist => await _context.Artists
            .FirstOrDefaultAsync(a => a.Id == report.TargetId),
        ReportType.User => await _userManager.FindByIdAsync(report.TargetId.ToString()),
        _ => null
    };
}
```

```csharp
// Cập nhật trạng thái báo cáo
[HttpPost]
public async Task<IActionResult> UpdateStatus(int id, ReportStatus status, string adminNote)
{
    var report = await _context.Reports.FindAsync(id);
    report.Status = status;
    report.AdminNote = adminNote;
    
    if (status == ReportStatus.Resolved || status == ReportStatus.Rejected)
    {
        report.ResolvedAt = DateTime.Now;
        report.ResolvedBy = User.Identity.Name;
    }
    
    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Details), new { id });
}
```

---

## Frontend (FE)

### 1. Users Views

#### Users/Index.cshtml
**Chức năng**: Hiển thị danh sách người dùng dạng bảng

**Tính năng**:
- Tìm kiếm theo tên hoặc email
- Hiển thị avatar, role, trạng thái
- Toggle Status và Premium bằng AJAX
- Badge hiển thị Premium/Active status
- Phân trang

**UI Components**:
```html
<!-- User Status Toggle -->
<button class="btn btn-toggle" onclick="toggleStatus('@user.Id')">
    <i class="bi @(user.IsActive ? "bi-check-circle text-success" : "bi-x-circle text-danger")"></i>
</button>

<!-- Premium Toggle -->
<button class="btn btn-toggle" onclick="togglePremium('@user.Id')">
    <i class="bi @(user.IsPremium ? "bi-star-fill text-warning" : "bi-star text-muted")"></i>
</button>
```

**AJAX Functions**:
```javascript
async function toggleStatus(id) {
    const response = await fetch(`/Admin/Users/ToggleStatus/${id}`, {
        method: 'POST',
        headers: { 'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]').value }
    });
    if (response.ok) location.reload();
}
```

#### Users/Details.cshtml
**Chức năng**: Hiển thị chi tiết profile người dùng

**Layout**:
- Avatar lớn với badge Premium
- Thông tin cơ bản (email, display name, bio)
- Thống kê hoạt động
- Lịch sử đăng nhập
- Action buttons (Toggle Status, Delete)

---

### 2. Reports Views

#### Reports/Index.cshtml
**Chức năng**: Danh sách báo cáo với bộ lọc

**Tính năng**:
- Lọc theo ReportType (Song, Album, Artist, User, Bug, Other)
- Lọc theo ReportStatus (Pending, InProgress, Resolved, Rejected)
- Badge màu sắc theo loại và trạng thái
- Link đến chi tiết

**Filter UI**:
```html
<select name="type" class="form-select">
    <option value="">-- Loại báo cáo --</option>
    <option value="Song">Bài hát</option>
    <option value="Album">Album</option>
    <option value="Artist">Nghệ sĩ</option>
    <option value="User">Người dùng</option>
    <option value="Bug">Lỗi</option>
    <option value="Other">Khác</option>
</select>
```

#### Reports/Details.cshtml
**Chức năng**: Chi tiết báo cáo và xử lý

**Layout**:
| Phần | Nội dung |
|------|----------|
| Main Content | Tiêu đề, mô tả, đối tượng báo cáo |
| Target Info | Thông tin bài hát/album/artist/user được báo cáo |
| Update Form | Các nút cập nhật trạng thái + textarea ghi chú |
| Sidebar | Thông tin, ngày tạo, người xử lý |
| Reporter | Thông tin người gửi báo cáo |

**Status Update UI**:
```html
<div class="d-flex gap-2 flex-wrap">
    <button type="submit" name="status" value="Pending" 
            class="btn @(Model.Status == ReportStatus.Pending ? "btn-warning" : "btn-outline-warning")">
        <i class="bi bi-clock me-1"></i>Chờ xử lý
    </button>
    <!-- Similar buttons for InProgress, Resolved, Rejected -->
</div>
```

---

## CSS Styles

### Thêm vào admin.css

```css
/* User/Report specific styles */
.target-item {
    display: flex;
    gap: 16px;
    align-items: center;
}

.target-cover {
    width: 80px;
    height: 80px;
    object-fit: cover;
    border-radius: var(--ml-radius);
}

.reporter-info {
    display: flex;
    gap: 16px;
    align-items: center;
}

.reporter-info img {
    width: 60px;
    height: 60px;
    border-radius: 50%;
    object-fit: cover;
}

.detail-stats {
    list-style: none;
    padding: 0;
}

.detail-stats li {
    display: flex;
    justify-content: space-between;
    padding: 12px 0;
    border-bottom: 1px solid var(--ml-border);
}
```

---

## Database & Models

### ReportType Enum
```csharp
public enum ReportType
{
    Song,
    Album,
    Artist,
    User,
    Bug,
    Other
}
```

### ReportStatus Enum
```csharp
public enum ReportStatus
{
    Pending,
    InProgress,
    Resolved,
    Rejected
}
```

### Report Model Properties
| Property | Type | Mô tả |
|----------|------|-------|
| Id | int | Primary key |
| UserId | string | FK đến người báo cáo |
| Type | ReportType | Loại báo cáo |
| TargetId | int | ID của đối tượng được báo cáo |
| Title | string | Tiêu đề |
| Description | string | Mô tả chi tiết |
| Status | ReportStatus | Trạng thái |
| AdminNote | string | Ghi chú của admin |
| CreatedAt | DateTime | Ngày tạo |
| ResolvedAt | DateTime? | Ngày xử lý |
| ResolvedBy | string | Admin xử lý |

---

## Luồng xử lý

### 1. Quản lý User
```
Admin vào Users Index
    ↓
Xem danh sách + tìm kiếm
    ↓
Toggle Status/Premium (AJAX)
    ↓
Hoặc xem Details
    ↓
Xóa user (nếu không phải Admin)
```

### 2. Xử lý Report
```
Admin vào Reports Index
    ↓
Lọc theo Type/Status
    ↓
Xem Details
    ↓
Xem đối tượng được báo cáo
    ↓
Chọn trạng thái mới + ghi chú
    ↓
Submit → cập nhật + ghi ResolvedAt, ResolvedBy
```

---

## Screenshots (Layout Description)

### Users Index
```
┌─────────────────────────────────────────────────────────────┐
│ Quản lý người dùng                        [Tổng: 50 users]  │
├─────────────────────────────────────────────────────────────┤
│ [🔍 Search input...                              ] [Search] │
├─────────────────────────────────────────────────────────────┤
│ Avatar | Tên         | Email       | Role  | Status | Prem  │
│ ○      | John Doe    | john@...    | User  | ●✓     | ⭐    │
│ ○      | Jane Smith  | jane@...    | Admin | ●✓     | ⭐    │
├─────────────────────────────────────────────────────────────┤
│ [← Prev]                                         [Next →]   │
└─────────────────────────────────────────────────────────────┘
```

### Reports Details
```
┌─────────────────────────────────────────────────────────────┐
│ Chi tiết báo cáo #123                           [🗑 Delete] │
├───────────────────────────────────────┬─────────────────────┤
│ Nội dung báo cáo           [🎵 Song]  │ Thông tin           │
│ ─────────────────────────────────────  │ ───────────────      │
│ Bài hát có vấn đề bản quyền           │ Status: Pending     │
│                                        │ Type: Song          │
│ [Mô tả chi tiết...]                   │ Created: 12/01/2025 │
├────────────────────────────────────────┼─────────────────────┤
│ Đối tượng báo cáo                     │ Người báo cáo       │
│ ─────────────────                      │ ─────────────        │
│ [🎵 Cover] Song Title                 │ [○] User Name       │
│            Artist Name                 │     email@...       │
│            [View Details]              │     [View Profile]  │
├────────────────────────────────────────┴─────────────────────┤
│ Xử lý báo cáo                                               │
│ [Chờ xử lý] [Đang xử lý] [Đã giải quyết] [Từ chối]         │
│ [Ghi chú Admin: __________________________]                 │
└─────────────────────────────────────────────────────────────┘
```

---

## Security Considerations

1. **Authorization**: `[Authorize(Roles = "Admin")]` trên tất cả controllers
2. **CSRF Protection**: `@Html.AntiForgeryToken()` trên tất cả forms
3. **Admin Protection**: Không cho phép xóa tài khoản có role Admin
4. **Input Validation**: Validate user ID trước khi thực hiện actions

---

## Files Created

| File | Loại | Mô tả |
|------|------|-------|
| `Areas/Admin/Controllers/UsersController.cs` | Controller | Quản lý users |
| `Areas/Admin/Controllers/ReportsController.cs` | Controller | Quản lý reports |
| `Areas/Admin/Views/Users/Index.cshtml` | View | Danh sách users |
| `Areas/Admin/Views/Users/Details.cshtml` | View | Chi tiết user |
| `Areas/Admin/Views/Reports/Index.cshtml` | View | Danh sách reports |
| `Areas/Admin/Views/Reports/Details.cshtml` | View | Chi tiết report |

---

## Next Tasks

- **Task 8**: Customer Home Page - Trang chủ khách hàng với banner, trending, new releases
- **Task 9**: Customer Songs & Albums - Các trang danh sách và chi tiết
- **Task 10**: Artists & Playlists - Trang nghệ sĩ và playlist của user

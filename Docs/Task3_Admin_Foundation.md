# Task 3: Admin Area Foundation

## Tổng quan
Task này thiết lập nền tảng cho khu vực Admin bao gồm layout riêng, dashboard với thống kê, và cấu trúc thư mục cho Area.

## Các file đã tạo

### 1. Controllers

#### `Areas/Admin/Controllers/DashboardController.cs`
Controller chính cho Dashboard Admin với các tính năng:

```csharp
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
```

**Phương thức Index:**
- Lấy thống kê tổng quan (tổng số bài hát, album, nghệ sĩ, người dùng)
- Đếm số mới trong tháng (bài hát mới, người dùng mới)
- Đếm báo cáo chờ xử lý
- Lấy Top 10 bài hát theo PlayCount
- Lấy 5 bài hát mới nhất
- Lấy 5 người dùng mới nhất
- Lấy báo cáo chờ xử lý
- Tính toán dữ liệu chart 6 tháng gần nhất

**Dữ liệu Chart:**
- SongUploadsPerMonth: Số bài hát upload mỗi tháng
- UserRegistrationsPerMonth: Số người dùng đăng ký mỗi tháng
- MonthLabels: Nhãn tháng (VD: "Th1", "Th2"...)

### 2. Views

#### `Areas/Admin/Views/_ViewStart.cshtml`
```cshtml
@{
    Layout = "_AdminLayout";
}
```

#### `Areas/Admin/Views/_ViewImports.cshtml`
```cshtml
@using WebListenMusic
@using WebListenMusic.Models
@using WebListenMusic.Models.ViewModels
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

#### `Areas/Admin/Views/Shared/_AdminLayout.cshtml`
Layout riêng cho Admin Area với:

**Sidebar Navigation:**
- Logo và brand
- Menu items: Dashboard, Bài hát, Albums, Nghệ sĩ, Thể loại, Người dùng, Báo cáo
- Link quay về trang chính
- Nút đăng xuất

**Header:**
- Mobile menu toggle
- Search bar
- User avatar & dropdown menu

**Content Area:**
- Main content với padding
- Render @RenderBody()

**Assets:**
- CSS: theme.css, admin.css
- JS: admin.js
- Bootstrap Icons
- Inter font

#### `Areas/Admin/Views/Dashboard/Index.cshtml`
Trang Dashboard chính với:

**Stats Cards:**
- Tổng số bài hát (với số mới trong tháng)
- Tổng số album
- Tổng số nghệ sĩ
- Tổng số người dùng (với số mới trong tháng)
- Báo cáo chờ xử lý (nếu có)

**Dashboard Grid (2 cột):**

*Main Column:*
- Chart thống kê 6 tháng (Chart.js)
- Bảng Top 10 bài hát (tên, lượt nghe, yêu thích)

*Sidebar Column:*
- Quick Actions (Thêm bài hát, album, nghệ sĩ)
- Danh sách bài hát mới (5 items)
- Danh sách người dùng mới (5 items)
- Danh sách báo cáo chờ xử lý

### 3. CSS

#### `wwwroot/css/admin.css`
Styles riêng cho Admin Area:

**Layout Styles:**
- `.admin-container`: Grid layout 2 columns
- `.admin-sidebar`: Fixed sidebar 260px
- `.admin-main`: Main content area
- `.admin-header`: Sticky header
- `.admin-content`: Content padding

**Dashboard Components:**
- `.dashboard-stats`: Grid stats cards
- `.stat-card`: Card với icon, số liệu, trend
- `.stat-icon`: Icons với màu sắc (blue, green, purple, orange, red)
- `.dashboard-grid`: 2 column layout cho dashboard content

**Data Components:**
- `.admin-card`: Card container
- `.admin-table`: Styled data table
- `.recent-list`, `.recent-item`: Danh sách items gần đây
- `.status-badge`: Badge trạng thái (active, inactive, pending, verified)
- `.activity-icon`: Icon hoạt động với màu

**Form Components:**
- `.form-page`: Form layout
- `.file-upload-area`: Drag & drop upload
- `.file-preview`: Preview uploaded files

**Quick Actions:**
- `.quick-actions`: Grid buttons
- `.quick-action-btn`: Action button với icon

**Responsive:**
- Mobile: Sidebar overlay với toggle
- Tablet: Adjusted grid columns

### 4. JavaScript

#### `wwwroot/js/admin.js`
JavaScript cho Admin Area:

**initMobileMenu():**
- Toggle sidebar trên mobile
- Close khi click outside

**initFileUploads():**
- Drag & drop file upload
- Preview image/audio files
- Display file info (name, size)

**initDeleteConfirmation():**
- Confirm dialog trước khi xóa
- Custom message từ data attribute

**initBulkActions():**
- Select all checkbox
- Show/hide bulk actions toolbar
- Count selected items

**initCharts():**
- Initialize Chart.js line chart
- Lấy data từ data attributes
- Config chart với dark theme colors

## Cấu trúc thư mục

```
Areas/
└── Admin/
    ├── Controllers/
    │   └── DashboardController.cs
    └── Views/
        ├── _ViewImports.cshtml
        ├── _ViewStart.cshtml
        ├── Shared/
        │   └── _AdminLayout.cshtml
        └── Dashboard/
            └── Index.cshtml

wwwroot/
├── css/
│   └── admin.css
└── js/
    └── admin.js
```

## Routing

Route cho Admin Area đã được cấu hình trong `Program.cs`:

```csharp
// Admin area route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
```

## Access Control

- Area yêu cầu đăng nhập và role "Admin"
- Sử dụng `[Area("Admin")]` và `[Authorize(Roles = "Admin")]` attributes

## Dependencies

- **Chart.js**: Thư viện chart (loaded từ CDN trong view)
- **Bootstrap Icons**: Icon library
- **Inter font**: Typography

## URL Pattern

| URL | Controller | Action |
|-----|------------|--------|
| /Admin | Dashboard | Index |
| /Admin/Dashboard | Dashboard | Index |
| /Admin/Songs | Songs | Index |
| /Admin/Albums | Albums | Index |
| /Admin/Artists | Artists | Index |
| /Admin/Users | Users | Index |
| /Admin/Reports | Reports | Index |

## Screenshots (Mô tả)

### Dashboard
- Header với search và user menu
- Sidebar với navigation
- Stats cards hiển thị số liệu tổng quan
- Chart thống kê 6 tháng
- Top 10 bài hát
- Quick actions
- Recent items lists

## Ghi chú kỹ thuật

1. **Responsive Design**: 
   - Desktop: Sidebar fixed, content bên phải
   - Mobile: Sidebar overlay với toggle button

2. **Dark Theme**: 
   - Sử dụng CSS variables từ theme.css
   - Consistent với design system

3. **Performance**:
   - Async data fetching trong controller
   - Lazy loading cho chart

4. **Security**:
   - Authorization check ở controller level
   - Role-based access control

## Tasks tiếp theo

- **Task 4**: Admin Songs CRUD - Quản lý bài hát
- **Task 5**: Admin Albums CRUD - Quản lý album
- **Task 6**: Admin Artists CRUD - Quản lý nghệ sĩ
- **Task 7**: Admin Users & Reports - Quản lý người dùng và báo cáo

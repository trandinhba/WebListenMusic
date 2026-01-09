# Task 13: Authentication & Genres - Chi tiết triển khai

## 📋 Tổng quan

Task 13 triển khai hệ thống xác thực người dùng (Authentication) hoàn chỉnh và quản lý thể loại nhạc (Genres) trong Admin Area.

---

## 📁 Các file được tạo

### 1. Authentication System

#### Controllers/AccountController.cs

**Mục đích:** Xử lý tất cả các chức năng liên quan đến xác thực người dùng.

**Dependencies:**
```csharp
using Microsoft.AspNetCore.Identity;
using WebListenMusic.Models;
using WebListenMusic.Models.ViewModels;
```

**Các Actions:**

| Action | Method | Route | Mô tả | Authorization |
|--------|--------|-------|-------|---------------|
| `Login` | GET | `/Account/Login` | Hiển thị form đăng nhập | AllowAnonymous |
| `Login` | POST | `/Account/Login` | Xử lý đăng nhập | AllowAnonymous |
| `Register` | GET | `/Account/Register` | Hiển thị form đăng ký | AllowAnonymous |
| `Register` | POST | `/Account/Register` | Xử lý đăng ký | AllowAnonymous |
| `Logout` | POST | `/Account/Logout` | Đăng xuất | Authenticated |
| `Lockout` | GET | `/Account/Lockout` | Trang thông báo bị khóa | AllowAnonymous |
| `AccessDenied` | GET | `/Account/AccessDenied` | Trang từ chối truy cập | AllowAnonymous |
| `ChangePassword` | GET/POST | `/Account/ChangePassword` | Đổi mật khẩu | Authorize |

**Luồng xử lý Login:**
```
1. Kiểm tra ModelState.IsValid
2. Tìm user theo Email
3. Kiểm tra IsActive (tài khoản bị khóa?)
4. Gọi SignInManager.PasswordSignInAsync()
5. Nếu thành công: Cập nhật LastLoginAt → Redirect
6. Nếu thất bại: Hiển thị lỗi
```

**Luồng xử lý Register:**
```
1. Kiểm tra Email đã tồn tại chưa
2. Tạo ApplicationUser mới
3. Gọi UserManager.CreateAsync()
4. Gán role "User" mặc định
5. Thêm Claims (DisplayName, AvatarUrl)
6. Tự động đăng nhập
7. Redirect về trang chủ
```

---

### 2. Account Views

#### Views/Account/Login.cshtml

**Layout:** Standalone (không dùng _Layout.cshtml)

**UI Components:**
- Logo MusicListen
- Form floating inputs (Email, Password)
- Checkbox "Ghi nhớ đăng nhập"
- Link "Quên mật khẩu"
- Button đăng nhập
- Divider "hoặc"
- Button đăng nhập Google (disabled)
- Link đăng ký

**Tính năng:**
- Toggle hiển thị mật khẩu (👁)
- Auto-focus vào email
- Hiển thị validation errors
- Responsive design
- Dark theme

---

#### Views/Account/Register.cshtml

**UI Components:**
- Form đăng ký với các trường:
  - Tên hiển thị
  - Email
  - Mật khẩu (với toggle show/hide)
  - Xác nhận mật khẩu
- Password strength indicator
- Checkbox đồng ý điều khoản
- Link chuyển sang đăng nhập

**JavaScript Features:**
```javascript
// Kiểm tra độ mạnh mật khẩu
function checkPasswordStrength(password) {
    - Kiểm tra độ dài >= 6
    - Kiểm tra có chữ thường
    - Kiểm tra có chữ số
    → Cập nhật progress bar và requirement icons
}

// Toggle hiển thị mật khẩu
function togglePassword(inputId, button) {
    - Chuyển type password ↔ text
    - Đổi icon eye ↔ eye-slash
}
```

---

#### Views/Account/AccessDenied.cshtml

**Mục đích:** Hiển thị khi user không có quyền truy cập trang

**UI:**
- Icon shield-lock (đỏ)
- Thông báo "Truy cập bị từ chối"
- Buttons: Về trang chủ, Đăng xuất/Đăng nhập

---

#### Views/Account/Lockout.cshtml

**Mục đích:** Hiển thị khi tài khoản bị khóa do nhập sai mật khẩu nhiều lần

**UI:**
- Icon lock (vàng)
- Thông báo tạm khóa (15 phút)
- Buttons: Về trang chủ, Thử lại

---

#### Views/Account/ChangePassword.cshtml

**Mục đích:** Cho phép user đổi mật khẩu

**Form Fields:**
- Mật khẩu hiện tại
- Mật khẩu mới
- Xác nhận mật khẩu mới

---

### 3. Admin Genres CRUD

#### Areas/Admin/Controllers/GenresController.cs

**Actions:**

| Action | Method | Route | Mô tả |
|--------|--------|-------|-------|
| `Index` | GET | `/Admin/Genres` | Danh sách thể loại |
| `Details` | GET | `/Admin/Genres/Details/{id}` | Chi tiết thể loại |
| `Create` | GET/POST | `/Admin/Genres/Create` | Thêm thể loại |
| `Edit` | GET/POST | `/Admin/Genres/Edit/{id}` | Sửa thể loại |
| `Delete` | GET/POST | `/Admin/Genres/Delete/{id}` | Xóa thể loại |

**Validation:**
- Kiểm tra trùng tên thể loại
- Không cho xóa nếu còn bài hát thuộc thể loại

---

#### Views/Admin/Genres/

| File | Mô tả |
|------|-------|
| `Index.cshtml` | Danh sách với search, pagination |
| `Create.cshtml` | Form thêm với color picker và preview |
| `Edit.cshtml` | Form sửa với preview realtime |
| `Details.cshtml` | Chi tiết + danh sách bài hát |
| `Delete.cshtml` | Xác nhận xóa với cảnh báo |

---

## 🎨 Styling

### Auth Pages CSS (inline trong view)

```css
/* Auth Page Container */
.auth-page {
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--ml-gradient-bg);
}

/* Auth Card */
.auth-card {
    background: var(--ml-surface);
    border: 1px solid var(--ml-border);
    border-radius: var(--ml-radius-xl);
    padding: 40px;
    max-width: 420px;
}

/* Floating Labels */
.form-floating > .form-control {
    background: var(--ml-bg);
    border: 1px solid var(--ml-border);
    color: var(--ml-text);
}

/* Password Toggle */
.password-toggle {
    position: absolute;
    right: 16px;
    top: 50%;
    transform: translateY(-50%);
}
```

---

## 🔐 Security Features

### 1. Password Requirements
```csharp
options.Password.RequireDigit = true;
options.Password.RequireLowercase = true;
options.Password.RequireUppercase = false;
options.Password.RequireNonAlphanumeric = false;
options.Password.RequiredLength = 6;
```

### 2. Lockout Policy
- Kích hoạt sau khi nhập sai nhiều lần
- Thời gian khóa mặc định: 15 phút

### 3. Cookie Settings
```csharp
options.LoginPath = "/Account/Login";
options.LogoutPath = "/Account/Logout";
options.AccessDeniedPath = "/Account/AccessDenied";
options.ExpireTimeSpan = TimeSpan.FromDays(30);
options.SlidingExpiration = true;
```

---

## 📊 ViewModels Sử Dụng

### LoginViewModel
```csharp
public class LoginViewModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public bool RememberMe { get; set; }
}
```

### RegisterViewModel
```csharp
public class RegisterViewModel
{
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}
```

### ChangePasswordViewModel
```csharp
public class ChangePasswordViewModel
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
}
```

---

## 🔗 API Routes Summary

### Authentication
| Method | Route | Mô tả |
|--------|-------|-------|
| GET | `/Account/Login` | Trang đăng nhập |
| POST | `/Account/Login` | Xử lý đăng nhập |
| GET | `/Account/Register` | Trang đăng ký |
| POST | `/Account/Register` | Xử lý đăng ký |
| POST | `/Account/Logout` | Đăng xuất |
| GET | `/Account/AccessDenied` | Trang từ chối |
| GET | `/Account/Lockout` | Trang bị khóa |
| GET/POST | `/Account/ChangePassword` | Đổi mật khẩu |

### Admin Genres
| Method | Route | Mô tả |
|--------|-------|-------|
| GET | `/Admin/Genres` | Danh sách |
| GET | `/Admin/Genres/Details/{id}` | Chi tiết |
| GET/POST | `/Admin/Genres/Create` | Thêm mới |
| GET/POST | `/Admin/Genres/Edit/{id}` | Chỉnh sửa |
| GET/POST | `/Admin/Genres/Delete/{id}` | Xóa |

---

## 🧪 Test Scenarios

### Authentication Tests
1. ✅ Đăng nhập thành công → Redirect về trang chủ
2. ✅ Đăng nhập sai password → Hiển thị lỗi
3. ✅ Đăng nhập account bị khóa → Thông báo tài khoản bị khóa
4. ✅ Đăng ký email mới → Tạo account và auto login
5. ✅ Đăng ký email đã tồn tại → Hiển thị lỗi
6. ✅ Đổi mật khẩu → Cập nhật và redirect

### Genres Admin Tests
1. ✅ Thêm thể loại mới → Hiển thị trong danh sách
2. ✅ Thêm thể loại trùng tên → Báo lỗi
3. ✅ Sửa thể loại → Cập nhật thành công
4. ✅ Xóa thể loại không có bài hát → Xóa thành công
5. ✅ Xóa thể loại có bài hát → Không cho xóa

---

## 📱 Responsive Design

### Auth Pages
- Desktop: Card centered, max-width 420px
- Mobile: Full width với padding

### Admin Genres
- Desktop: Table view
- Mobile: Stacked cards (nếu cần)

---

## 🔧 Cấu hình cần thiết

### Program.cs đã có sẵn:
```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => { ... })
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
```

---

## 📈 Cải tiến có thể thêm

1. **Email Confirmation** - Xác nhận email khi đăng ký
2. **Forgot Password** - Khôi phục mật khẩu qua email
3. **External Login** - Đăng nhập qua Google, Facebook
4. **Two-Factor Auth** - Xác thực 2 bước
5. **Remember Device** - Ghi nhớ thiết bị tin cậy
6. **Login History** - Lịch sử đăng nhập
7. **Session Management** - Quản lý phiên đăng nhập

---

## 📝 Ghi chú bảo trì

### Khi cần thêm role mới:
1. Thêm role trong `SeedData.cs`
2. Cập nhật `[Authorize(Roles = "...")]` trong controllers

### Khi cần thêm claims:
```csharp
// Trong AccountController.Register()
var claims = new List<Claim>
{
    new Claim("ClaimName", "ClaimValue")
};
await _userManager.AddClaimsAsync(user, claims);
```

### Khi cần tùy chỉnh password policy:
```csharp
// Trong Program.cs
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    // ...
});
```

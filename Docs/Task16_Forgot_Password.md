# 📋 Task 16: Forgot Password (Quên Mật Khẩu)

## 🎯 Mục tiêu
Implement tính năng quên mật khẩu cho phép user reset password qua email hoặc link trực tiếp.

---

## 📋 Điều kiện tiên quyết

### Đã có sẵn ✅
- ASP.NET Core Identity
- `ForgotPasswordViewModel` và `ResetPasswordViewModel` trong AccountViewModels.cs
- UserManager và SignInManager

### Cần thêm (Tùy chọn)
- Email Service (SMTP) - Để gửi email thực
- Hoặc dùng Development Mode hiển thị link trực tiếp

---

## 🔄 Quy trình hoạt động

```
┌─────────────────────────────────────────────────────────────────────┐
│                                                                     │
│  User quên MK → Nhập email → Tạo token → Gửi/Hiện link reset       │
│                                                     ↓               │
│  Đăng nhập ← Đổi MK thành công ← Nhập MK mới ← Click link          │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## ✅ Các công việc cần thực hiện

### 1. ViewModels (Đã có sẵn ✅)

#### 📁 Models/ViewModels/AccountViewModels.cs
```csharp
public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, ErrorMessage = "{0} must be at least {2} and at most {1} characters.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? Code { get; set; }
}
```

---

### 2. Controller Actions

#### 📁 Controllers/AccountController.cs

**Thêm các actions sau:**

```csharp
#region Forgot Password

// GET: /Account/ForgotPassword
[HttpGet]
[AllowAnonymous]
public IActionResult ForgotPassword()
{
    return View();
}

// POST: /Account/ForgotPassword
[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
{
    if (ModelState.IsValid)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        
        // Không tiết lộ user có tồn tại hay không (bảo mật)
        if (user == null || !user.IsActive)
        {
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        // Tạo token reset password
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        // Tạo link reset password
        var callbackUrl = Url.Action(
            "ResetPassword",
            "Account",
            new { email = user.Email, code = code },
            protocol: Request.Scheme);

        // Development: Hiển thị link trực tiếp
        // Production: Gửi email thực
        TempData["ResetLink"] = callbackUrl;
        
        // TODO: Gửi email nếu có Email Service
        // await _emailService.SendEmailAsync(user.Email, "Reset Password", 
        //     $"Click <a href='{callbackUrl}'>here</a> to reset password.");
        
        _logger.LogInformation("Password reset link generated for {Email}", model.Email);
        
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    return View(model);
}

// GET: /Account/ForgotPasswordConfirmation
[HttpGet]
[AllowAnonymous]
public IActionResult ForgotPasswordConfirmation()
{
    return View();
}

#endregion

#region Reset Password

// GET: /Account/ResetPassword
[HttpGet]
[AllowAnonymous]
public IActionResult ResetPassword(string? email = null, string? code = null)
{
    if (code == null || email == null)
    {
        return BadRequest("Invalid password reset link.");
    }
    
    var model = new ResetPasswordViewModel 
    { 
        Code = code,
        Email = email
    };
    return View(model);
}

// POST: /Account/ResetPassword
[HttpPost]
[AllowAnonymous]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    var user = await _userManager.FindByEmailAsync(model.Email);
    if (user == null)
    {
        // Không tiết lộ user không tồn tại
        return RedirectToAction(nameof(ResetPasswordConfirmation));
    }

    var result = await _userManager.ResetPasswordAsync(user, model.Code!, model.Password);
    
    if (result.Succeeded)
    {
        _logger.LogInformation("Password reset successful for {Email}", model.Email);
        return RedirectToAction(nameof(ResetPasswordConfirmation));
    }

    foreach (var error in result.Errors)
    {
        ModelState.AddModelError(string.Empty, TranslateError(error.Code));
    }
    
    return View(model);
}

// GET: /Account/ResetPasswordConfirmation
[HttpGet]
[AllowAnonymous]
public IActionResult ResetPasswordConfirmation()
{
    return View();
}

#endregion
```

---

### 3. Views

#### 📁 Views/Account/ForgotPassword.cshtml
```html
@model ForgotPasswordViewModel
@{
    ViewData["Title"] = "Forgot Password";
    Layout = "~/Views/Shared/_AuthLayout.cshtml";
}

<div class="auth-card">
    <div class="auth-header">
        <div class="auth-logo">
            <i class="bi bi-music-note-beamed"></i>
        </div>
        <h1>Forgot Password</h1>
        <p class="text-muted">Enter your email to reset your password</p>
    </div>

    <form asp-action="ForgotPassword" method="post">
        <div asp-validation-summary="ModelOnly" class="alert alert-danger"></div>
        
        <div class="form-floating mb-4">
            <input asp-for="Email" class="form-control" placeholder="name@example.com" />
            <label asp-for="Email"><i class="bi bi-envelope me-2"></i>Email</label>
            <span asp-validation-for="Email" class="text-danger"></span>
        </div>

        <button type="submit" class="btn btn-primary w-100 btn-lg mb-3">
            <i class="bi bi-send me-2"></i>Send Reset Link
        </button>

        <div class="text-center">
            <a asp-action="Login"><i class="bi bi-arrow-left me-1"></i>Back to Login</a>
        </div>
    </form>
</div>
```

#### 📁 Views/Account/ForgotPasswordConfirmation.cshtml
```html
@{
    ViewData["Title"] = "Check Your Email";
    Layout = "~/Views/Shared/_AuthLayout.cshtml";
}

<div class="auth-card text-center">
    <div class="auth-header">
        <div class="auth-logo" style="background: var(--ml-success); color: white;">
            <i class="bi bi-envelope-check"></i>
        </div>
        <h1>Check Your Email</h1>
        <p class="text-muted">If an account exists, we've sent a reset link.</p>
    </div>

    @if (TempData["ResetLink"] != null)
    {
        <div class="alert alert-info text-start mb-4">
            <strong><i class="bi bi-info-circle me-2"></i>Development Mode:</strong>
            <p class="mb-2 mt-2">Click link below to reset password:</p>
            <a href="@TempData["ResetLink"]" class="btn btn-primary btn-sm">
                <i class="bi bi-key me-2"></i>Reset Password
            </a>
        </div>
    }

    <a asp-action="Login" class="btn btn-primary">Back to Login</a>
</div>
```

#### 📁 Views/Account/ResetPassword.cshtml
```html
@model ResetPasswordViewModel
@{
    ViewData["Title"] = "Reset Password";
    Layout = "~/Views/Shared/_AuthLayout.cshtml";
}

<div class="auth-card">
    <div class="auth-header">
        <div class="auth-logo">
            <i class="bi bi-key"></i>
        </div>
        <h1>Reset Password</h1>
        <p class="text-muted">Enter your new password</p>
    </div>

    <form asp-action="ResetPassword" method="post">
        <div asp-validation-summary="ModelOnly" class="alert alert-danger"></div>
        
        <input asp-for="Code" type="hidden" />
        <input asp-for="Email" type="hidden" />
        
        <div class="mb-3">
            <label class="form-label text-muted small">Email</label>
            <p class="fw-medium">@Model.Email</p>
        </div>

        <div class="form-floating mb-3">
            <input asp-for="Password" class="form-control" placeholder="New password" />
            <label asp-for="Password"><i class="bi bi-lock me-2"></i>New Password</label>
            <span asp-validation-for="Password" class="text-danger"></span>
        </div>

        <div class="form-floating mb-4">
            <input asp-for="ConfirmPassword" class="form-control" placeholder="Confirm" />
            <label asp-for="ConfirmPassword"><i class="bi bi-lock-fill me-2"></i>Confirm Password</label>
            <span asp-validation-for="ConfirmPassword" class="text-danger"></span>
        </div>

        <button type="submit" class="btn btn-primary w-100 btn-lg">
            <i class="bi bi-check-lg me-2"></i>Reset Password
        </button>
    </form>
</div>
```

#### 📁 Views/Account/ResetPasswordConfirmation.cshtml
```html
@{
    ViewData["Title"] = "Password Reset!";
    Layout = "~/Views/Shared/_AuthLayout.cshtml";
}

<div class="auth-card text-center">
    <div class="auth-header">
        <div class="auth-logo" style="background: var(--ml-success); color: white;">
            <i class="bi bi-check-circle"></i>
        </div>
        <h1>Password Reset!</h1>
        <p class="text-muted">Your password has been successfully reset.</p>
    </div>

    <a asp-action="Login" class="btn btn-primary btn-lg w-100">
        <i class="bi bi-box-arrow-in-right me-2"></i>Login Now
    </a>
</div>
```

#### 📁 Views/Shared/_AuthLayout.cshtml (Nếu chưa có)
```html
@{
    Layout = null;
}
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - MusicListen</title>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.2/font/bootstrap-icons.min.css">
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/theme.css" asp-append-version="true" />
    <style>
        .auth-page { min-height: 100vh; display: flex; align-items: center; justify-content: center; background: var(--ml-gradient-bg); padding: 20px; }
        .auth-container { width: 100%; max-width: 450px; }
        .auth-card { background: var(--ml-surface); border: 1px solid var(--ml-border); border-radius: var(--ml-radius-xl); padding: 40px; box-shadow: var(--ml-shadow-lg); }
        .auth-header { text-align: center; margin-bottom: 32px; }
        .auth-logo { width: 64px; height: 64px; background: var(--ml-gradient-primary); border-radius: var(--ml-radius-lg); display: flex; align-items: center; justify-content: center; margin: 0 auto 16px; font-size: 1.75rem; color: white; }
        .auth-header h1 { font-size: 1.5rem; font-weight: 600; margin-bottom: 8px; }
        .form-floating { margin-bottom: 16px; }
        .form-floating > .form-control { background: var(--ml-bg); border: 1px solid var(--ml-border); color: var(--ml-text); height: 56px; }
        .form-floating > .form-control:focus { border-color: var(--ml-primary); box-shadow: 0 0 0 3px rgba(var(--ml-primary-rgb), 0.15); }
        .form-floating > label { color: var(--ml-text-muted); }
    </style>
</head>
<body>
    <div class="auth-page">
        <div class="auth-container">
            @RenderBody()
        </div>
    </div>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

---

### 4. Cập nhật Login.cshtml

**Sửa link "Forgot password?":**
```html
<!-- Từ -->
<a href="#" class="text-primary small">Forgot password?</a>

<!-- Thành -->
<a asp-action="ForgotPassword" class="text-primary small">Forgot password?</a>
```

---

## 🔧 Tùy chọn: Email Service

### Cấu hình appsettings.json
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderName": "MusicListen",
    "SenderEmail": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

### Tạo Email Service
```csharp
// Services/IEmailService.cs
public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}

// Services/EmailService.cs
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    
    public EmailService(IConfiguration config)
    {
        _config = config;
    }
    
    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var smtpServer = _config["EmailSettings:SmtpServer"];
        var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"]);
        var senderEmail = _config["EmailSettings:SenderEmail"];
        var password = _config["EmailSettings:Password"];
        
        using var client = new SmtpClient(smtpServer, smtpPort);
        client.EnableSsl = true;
        client.Credentials = new NetworkCredential(senderEmail, password);
        
        var message = new MailMessage(senderEmail, toEmail, subject, htmlBody);
        message.IsBodyHtml = true;
        
        await client.SendMailAsync(message);
    }
}
```

### Đăng ký trong Program.cs
```csharp
builder.Services.AddTransient<IEmailService, EmailService>();
```

---

## 🔐 Bảo mật

| Yêu cầu | Mô tả |
|---------|-------|
| Không tiết lộ user | Luôn redirect về Confirmation dù email có tồn tại hay không |
| Token một lần | Token chỉ dùng được 1 lần |
| Token có hạn | Mặc định 1 ngày (có thể cấu hình) |
| HTTPS | Link reset phải dùng HTTPS trong production |

---

## ✅ Checklist

- [ ] Thêm Controller Actions (ForgotPassword, ResetPassword)
- [ ] Tạo View ForgotPassword.cshtml
- [ ] Tạo View ForgotPasswordConfirmation.cshtml
- [ ] Tạo View ResetPassword.cshtml
- [ ] Tạo View ResetPasswordConfirmation.cshtml
- [ ] Tạo _AuthLayout.cshtml (nếu chưa có)
- [ ] Cập nhật link trong Login.cshtml
- [ ] (Tùy chọn) Cấu hình Email Service
- [ ] Test tính năng

---

## 📊 Endpoints

| Method | URL | Mô tả |
|--------|-----|-------|
| GET | /Account/ForgotPassword | Form nhập email |
| POST | /Account/ForgotPassword | Tạo token + gửi/hiện link |
| GET | /Account/ForgotPasswordConfirmation | Thông báo đã gửi |
| GET | /Account/ResetPassword?email=&code= | Form nhập mật khẩu mới |
| POST | /Account/ResetPassword | Xử lý đổi mật khẩu |
| GET | /Account/ResetPasswordConfirmation | Thông báo thành công |

---

**Status:** ⏳ Chưa thực hiện
**Date:** 2025-01-07

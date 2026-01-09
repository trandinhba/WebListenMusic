# ?? TÀI LI?U CH?C N?NG - MUSICLISTEN

## ?? T?ng quan h? th?ng

**MusicListen** là ?ng d?ng web nghe nh?c tr?c tuy?n ???c xây d?ng trên n?n t?ng:
- **Framework:** ASP.NET Core 8.0 (MVC)
- **Database:** SQL Server v?i Entity Framework Core
- **Authentication:** ASP.NET Core Identity

---

## ?? CH?C N?NG NG??I DÙNG (USER)

### 1. ??ng ký / ??ng nh?p
| Ch?c n?ng | Mô t? | URL |
|-----------|-------|-----|
| ??ng ký | T?o tài kho?n m?i v?i email, m?t kh?u, tên hi?n th? | `/Account/Register` |
| ??ng nh?p | ??ng nh?p b?ng email và m?t kh?u | `/Account/Login` |
| ??ng xu?t | Thoát kh?i tài kho?n | POST `/Account/Logout` |
| ??i m?t kh?u | Thay ??i m?t kh?u hi?n t?i | `/Account/ChangePassword` |

### 2. Qu?n lý Profile
| Ch?c n?ng | Mô t? | URL |
|-----------|-------|-----|
| Xem Profile | Xem thông tin cá nhân và playlist | `/Profile` |
| **Ch?nh s?a Profile** | C?p nh?t ?nh ??i di?n, tên hi?n th?, bio | `/Profile/Edit` |
| Cài ??t | Xem cài ??t tài kho?n | `/Profile/Settings` |

#### Chi ti?t ch?c n?ng Ch?nh s?a Profile:
- **Thay ??i ?nh ??i di?n (Avatar):**
  - Upload ?nh m?i (JPG, PNG, GIF, WebP)
  - ?nh c? s? t? ??ng b? xóa (tr? ?nh m?c ??nh)
  - ?nh ???c l?u t?i: `/uploads/avatars/`
  - **C?p nh?t Claims** ?? ?nh hi?n th? ngay ? sidebar mà không c?n ??ng nh?p l?i

- **Thay ??i tên hi?n th? (Display Name):**
  - T?i ?a 100 ký t?
  - Hi?n th? trên profile và sidebar

- **Thay ??i Bio:**
  - Mô t? ng?n v? b?n thân
  - T?i ?a 500 ký t?

### 3. Nghe nh?c
| Ch?c n?ng | Mô t? | URL |
|-----------|-------|-----|
| Xem danh sách bài hát | Duy?t t?t c? bài hát | `/Songs` |
| Chi ti?t bài hát | Xem thông tin, lyrics, ?ánh giá, bình lu?n | `/Songs/Details/{id}` |
| Phát nh?c | Phát bài hát v?i player tích h?p | Click vào bài hát |
| Xem Album | Xem danh sách album | `/Albums` |
| Xem Ngh? s? | Xem danh sách ngh? s? | `/Artists` |
| Tìm ki?m | Tìm bài hát, album, ngh? s? | `/Search` |

### 4. Playlist
| Ch?c n?ng | Mô t? | URL |
|-----------|-------|-----|
| Xem Playlist | Xem danh sách playlist | `/Playlists` |
| T?o Playlist | T?o playlist m?i | `/Playlists/Create` |
| Ch?nh s?a Playlist | S?a tên, mô t?, ?nh bìa | `/Playlists/Edit/{id}` |
| Thêm bài hát vào Playlist | Thêm bài hát t? b?t k? trang nào | Modal popup |
| Xóa Playlist | Xóa playlist c?a mình | `/Playlists/Delete/{id}` |

### 5. Yêu thích & L?ch s?
| Ch?c n?ng | Mô t? | URL |
|-----------|-------|-----|
| Bài hát yêu thích | Xem danh sách bài hát ?ã thích | `/Profile/Favorites` |
| Thêm/Xóa yêu thích | Toggle yêu thích bài hát | POST `/Profile/ToggleFavorite` |
| L?ch s? nghe | Xem l?ch s? nghe nh?c | `/Profile/History` |
| Xóa l?ch s? | Xóa toàn b? l?ch s? nghe | POST `/Profile/ClearHistory` |

### 6. ?ánh giá & Bình lu?n
| Ch?c n?ng | Mô t? | URL |
|-----------|-------|-----|
| ?ánh giá bài hát | ?ánh giá 1-5 sao | POST `/Songs/Rate` |
| Bình lu?n | Vi?t bình lu?n cho bài hát | POST `/Songs/Comment` |
| S?a bình lu?n | Ch?nh s?a bình lu?n c?a mình | POST `/Songs/EditComment` |
| Xóa bình lu?n | Xóa bình lu?n c?a mình | POST `/Songs/DeleteComment` |

---

## ??? CH?C N?NG QU?N TR? (ADMIN)

### 1. Dashboard
| Ch?c n?ng | Mô t? | URL |
|-----------|-------|-----|
| T?ng quan | Th?ng kê s? l??ng, bi?u ?? | `/Admin/Dashboard` |

### 2. Qu?n lý Bài hát
| Ch?c n?ng | Mô t? | URL |
|-----------|-------|-----|
| Danh sách | Xem t?t c? bài hát | `/Admin/Songs` |
| Thêm m?i | Upload bài hát m?i (MP3, WAV, M4A, FLAC) | `/Admin/Songs/Create` |
| Ch?nh s?a | S?a thông tin bài hát | `/Admin/Songs/Edit/{id}` |
| Xóa | Xóa bài hát | `/Admin/Songs/Delete/{id}` |
| Chi ti?t | Xem chi ti?t bài hát | `/Admin/Songs/Details/{id}` |

### 3. Qu?n lý Album
| Ch?c n?ng | Mô t? | URL |
|-----------|-------|-----|
| Danh sách | Xem t?t c? album | `/Admin/Albums` |
| Thêm m?i | T?o album m?i | `/Admin/Albums/Create` |
| Ch?nh s?a | S?a thông tin album | `/Admin/Albums/Edit/{id}` |
| Xóa | Xóa album | `/Admin/Albums/Delete/{id}` |

### 4. Qu?n lý Ngh? s?
| Ch?c n?ng | Mô t? | URL |
|-----------|-------|-----|
| Danh sách | Xem t?t c? ngh? s? | `/Admin/Artists` |
| Thêm m?i | T?o ngh? s? m?i | `/Admin/Artists/Create` |
| Ch?nh s?a | S?a thông tin ngh? s? | `/Admin/Artists/Edit/{id}` |
| Xóa | Xóa ngh? s? | `/Admin/Artists/Delete/{id}` |

### 5. Qu?n lý Th? lo?i
| Ch?c n?ng | Mô t? | URL |
|-----------|-------|-----|
| Danh sách | Xem t?t c? th? lo?i | `/Admin/Genres` |
| Thêm m?i | T?o th? lo?i m?i | `/Admin/Genres/Create` |
| Ch?nh s?a | S?a thông tin th? lo?i | `/Admin/Genres/Edit/{id}` |
| Xóa | Xóa th? lo?i | `/Admin/Genres/Delete/{id}` |

### 6. Qu?n lý Ng??i dùng
| Ch?c n?ng | Mô t? | URL |
|-----------|-------|-----|
| Danh sách | Xem t?t c? ng??i dùng | `/Admin/Users` |
| Chi ti?t | Xem thông tin chi ti?t user | `/Admin/Users/Details/{id}` |
| Khóa/M? khóa | Toggle tr?ng thái active | POST `/Admin/Users/ToggleStatus` |
| Premium | Toggle tr?ng thái Premium | POST `/Admin/Users/TogglePremium` |
| Xóa | Xóa tài kho?n ng??i dùng | POST `/Admin/Users/Delete` |

### 7. Qu?n lý Báo cáo
| Ch?c n?ng | Mô t? | URL |
|-----------|-------|-----|
| Danh sách | Xem t?t c? báo cáo vi ph?m | `/Admin/Reports` |
| Chi ti?t | Xem chi ti?t báo cáo | `/Admin/Reports/Details/{id}` |
| X? lý | ?ánh d?u ?ã x? lý | POST `/Admin/Reports/Resolve` |

---

## ??? C?U TRÚC DATABASE

### Các b?ng chính:
| B?ng | Mô t? |
|------|-------|
| `AspNetUsers` | Thông tin ng??i dùng (Identity) |
| `AspNetRoles` | Vai trò (Admin, User) |
| `Songs` | Bài hát |
| `Albums` | Album |
| `Artists` | Ngh? s? |
| `Genres` | Th? lo?i nh?c |
| `Playlists` | Playlist c?a user |
| `PlaylistSongs` | Liên k?t Playlist - Song |
| `FavoriteSongs` | Bài hát yêu thích |
| `ListeningHistories` | L?ch s? nghe |
| `SongRatings` | ?ánh giá bài hát |
| `SongComments` | Bình lu?n bài hát |
| `Reports` | Báo cáo vi ph?m |

---

## ?? C?U TRÚC TH? M?C

```
WebListenMusic/
??? Areas/
?   ??? Admin/
?       ??? Controllers/     # Controllers cho Admin
?       ??? Views/           # Views cho Admin
??? Controllers/             # Controllers cho User
??? Data/
?   ??? ApplicationDbContext.cs
?   ??? SeedData.cs
??? Helpers/
?   ??? FileHelper.cs
??? Migrations/              # EF Core Migrations
??? Models/
?   ??? ViewModels/
?   ??? [Entity Models]
??? Views/                   # Views cho User
??? wwwroot/
?   ??? css/
?   ??? js/
?   ??? images/
?   ??? uploads/
?       ??? avatars/         # ?nh ??i di?n user
?       ??? songs/           # File nh?c
?       ??? covers/          # ?nh bìa
?       ??? artists/         # ?nh ngh? s?
??? Program.cs
```

---

## ?? TÀI KHO?N M?C ??NH

| Vai trò | Email | M?t kh?u |
|---------|-------|----------|
| Admin | admin@musiclisten.com | Admin@123 |

---

## ?? GHI CHÚ C?P NH?T

### Ngày 09/01/2026
- ? Thêm ch?c n?ng **Edit Profile** v?i c?p nh?t Claims
- ? ?nh ??i di?n c?p nh?t ngay ? sidebar sau khi ch?nh s?a
- ? Thêm link "Edit Profile" vào dropdown menu sidebar

### Ngày 07/01/2026
- ? Thêm b?ng `SongComments` và `SongRatings`
- ? Thêm b?ng `FavoriteSongs` và `ListeningHistories`
- ? Migration `AddCommentsAndRatings`
- ? Migration `AddFavoritesAndHistory`

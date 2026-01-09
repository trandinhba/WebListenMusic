# Task 14: Tính năng Embed Audio từ Nguồn Bên Ngoài

## 📋 Tổng quan

Tính năng này cho phép Admin thêm bài hát vào hệ thống bằng 2 cách:
1. **Upload file MP3** trực tiếp lên server
2. **Nhúng (Embed)** từ các nền tảng âm nhạc bên ngoài (Zing MP3, SoundCloud, Spotify...)

### Ưu điểm của Embed
- ✅ Không tốn dung lượng server
- ✅ Không lo vấn đề bản quyền
- ✅ Tận dụng CDN của nhà cung cấp
- ✅ Chất lượng âm thanh ổn định
- ✅ Phát trực tiếp từ Player Bar (không cần vào trang chi tiết)

---

## 🏗️ Kiến trúc

### 1. Model Layer

#### File: `Models/Song.cs`

```csharp
public class Song
{
    // ... existing properties ...
    
    // Đường dẫn file MP3 (dùng khi SourceType = Upload)
    public string? AudioUrl { get; set; }
    
    // Mã nhúng iframe (dùng khi SourceType != Upload)
    [StringLength(500)]
    public string? EmbedCode { get; set; }
    
    // Loại nguồn nhạc
    public AudioSourceType SourceType { get; set; } = AudioSourceType.Upload;
}

public enum AudioSourceType
{
    Upload = 0,      // Upload file MP3 lên server
    ZingMp3 = 1,     // Nhúng từ Zing MP3
    SoundCloud = 2,  // Nhúng từ SoundCloud
    Spotify = 3,     // Nhúng từ Spotify
    ExternalUrl = 4  // URL bên ngoài khác
}
```

### 2. ViewModel Layer

#### File: `Models/ViewModels/AdminViewModels.cs`

```csharp
public class AdminSongFormViewModel
{
    // ... existing properties ...
    
    // Embed support
    public AudioSourceType SourceType { get; set; } = AudioSourceType.Upload;
    public string? EmbedCode { get; set; }
    
    public IFormFile? AudioFile { get; set; }
    public IFormFile? CoverImage { get; set; }
}
```

### 3. Player Bar UI

#### File: `Views/Shared/_Player.cshtml`

Có 2 loại player bar:
- **Standard Player Bar**: Cho bài hát upload (MP3)
- **Embed Player Bar**: Cho bài hát embed (hiển thị iframe)

```html
<!-- Standard Audio Player Bar -->
<div class="player-bar" id="playerBar">
    <!-- Controls cho MP3 player -->
</div>

<!-- Embed Player Bar -->
<div class="player-bar player-bar-embed" id="playerBarEmbed" style="display: none;">
    <div class="player-embed-info">
        <!-- Cover + Title + Artist -->
    </div>
    <div class="player-embed-container" id="embedContainer">
        <!-- Iframe sẽ được chèn vào đây -->
    </div>
    <div class="player-embed-actions">
        <button onclick="closeEmbedPlayer()">✕</button>
    </div>
</div>
```

---

## 🔄 Flow Chi tiết

### Flow 1: Admin Thêm Bài Hát (Upload File)

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  Admin chọn     │     │  Controller xử   │     │  Lưu file vào   │
│  "Upload file"  │ ──> │  lý upload       │ ──> │  /uploads/songs │
│  + chọn MP3     │     │                  │     │                 │
└─────────────────┘     └──────────────────┘     └─────────────────┘
                                │
                                ▼
                        ┌──────────────────┐
                        │  Lưu AudioUrl    │
                        │  vào Database    │
                        │  SourceType = 0  │
                        └──────────────────┘
```

### Flow 2: Admin Thêm Bài Hát (Embed)

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  Admin chọn     │     │  Controller xử   │     │  Chuyển đổi URL │
│  "Zing MP3"     │ ──> │  lý EmbedCode    │ ──> │  thành iframe   │
│  + dán URL      │     │                  │     │  (nếu cần)      │
└─────────────────┘     └──────────────────┘     └─────────────────┘
                                │
                                ▼
                        ┌──────────────────┐
                        │  Lưu EmbedCode   │
                        │  vào Database    │
                        │  SourceType = 1  │
                        └──────────────────┘
```

### Flow 3: User Click Play Bài Upload (MP3)

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  User click     │     │  JS fetch        │     │  Kiểm tra       │
│  nút Play       │ ──> │  /Songs/GetData  │ ──> │  sourceType     │
└─────────────────┘     └──────────────────┘     └─────────────────┘
                                                        │
                                                        ▼ sourceType = 0
                                                ┌───────────────┐
                                                │ Phát bằng     │
                                                │ <audio> tag   │
                                                │ Player Bar    │
                                                │ bình thường   │
                                                └───────────────┘
```

### Flow 4: User Click Play Bài Embed

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  User click     │     │  JS fetch        │     │  Kiểm tra       │
│  nút Play       │ ──> │  /Songs/GetData  │ ──> │  sourceType     │
└─────────────────┘     └──────────────────┘     └─────────────────┘
                                                        │
                                                        ▼ sourceType != 0
                                                ┌───────────────┐
                                                │ Ẩn Standard   │
                                                │ Player Bar    │
                                                └───────┬───────┘
                                                        │
                                                        ▼
                                                ┌───────────────┐
                                                │ Hiện Embed    │
                                                │ Player Bar    │
                                                │ + Chèn iframe │
                                                └───────────────┘
```

---

## 💻 Code Implementation

### 1. Controller: Xử lý Create

#### File: `Areas/Admin/Controllers/SongsController.cs`

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(AdminSongFormViewModel model)
{
    // Bỏ qua validation AudioFile nếu dùng embed
    if (model.SourceType != AudioSourceType.Upload)
    {
        ModelState.Remove("AudioFile");
    }

    if (ModelState.IsValid)
    {
        var song = new Song
        {
            Title = model.Title,
            SourceType = model.SourceType,
            // ... other properties
        };

        // Xử lý theo loại nguồn
        if (model.SourceType == AudioSourceType.Upload)
        {
            // Upload file MP3
            if (model.AudioFile != null)
            {
                song.AudioUrl = await FileHelper.SaveFileAsync(...);
            }
        }
        else
        {
            // Xử lý embed code
            song.EmbedCode = ProcessEmbedCode(model.EmbedCode, model.SourceType);
        }

        _context.Songs.Add(song);
        await _context.SaveChangesAsync();
    }
}
```

### 2. Controller: API GetSongData

#### File: `Controllers/SongsController.cs`

```csharp
[HttpGet]
public async Task<IActionResult> GetSongData(int id)
{
    var song = await _context.Songs
        .Include(s => s.Artist)
        .FirstOrDefaultAsync(s => s.Id == id && s.IsPublished);

    if (song == null) return NotFound();

    return Json(new
    {
        id = song.Id,
        title = song.Title,
        artist = song.Artist?.Name ?? "Unknown",
        audioUrl = song.AudioUrl,
        coverUrl = song.CoverImageUrl ?? "/images/default-song.svg",
        duration = song.Duration,
        sourceType = (int)song.SourceType,  // Quan trọng!
        embedCode = song.EmbedCode           // Quan trọng!
    });
}
```

### 3. JavaScript: Player Logic

#### File: `wwwroot/js/player.js`

```javascript
// Global function to play a song by ID
async function playSong(songId) {
    const response = await fetch(`/Songs/GetSongData/${songId}`);
    const songData = await response.json();
    
    // Nếu là bài embed, hiển thị embed player bar
    if (songData.embedCode && songData.sourceType !== 0) {
        playEmbedSong(songData);
        return;
    }
    
    // Nếu là bài upload, phát bằng audio player
    if (songData.audioUrl) {
        document.getElementById('playerBarEmbed').style.display = 'none';
        document.getElementById('playerBar').style.display = 'grid';
        player.playSong(songData);
    }
}

// Play embed song - hiển thị iframe trong player bar
function playEmbedSong(songData) {
    const playerBar = document.getElementById('playerBar');
    const playerBarEmbed = document.getElementById('playerBarEmbed');
    const embedContainer = document.getElementById('embedContainer');
    
    // Pause audio player nếu đang phát
    if (player && player.audio) {
        player.audio.pause();
    }
    
    // Ẩn standard player, hiện embed player
    playerBar.style.display = 'none';
    playerBarEmbed.style.display = 'grid';
    
    // Update thông tin bài hát
    document.getElementById('playerCoverEmbed').src = songData.coverUrl;
    document.getElementById('playerTitleEmbed').textContent = songData.title;
    document.getElementById('playerArtistEmbed').textContent = songData.artist;
    
    // Chèn embed code (iframe)
    embedContainer.innerHTML = songData.embedCode;
}

// Đóng embed player, quay lại standard player
function closeEmbedPlayer() {
    document.getElementById('embedContainer').innerHTML = '';
    document.getElementById('playerBarEmbed').style.display = 'none';
    document.getElementById('playerBar').style.display = 'grid';
}
```

### 4. Helper Method: Chuyển đổi URL thành Embed

```csharp
private string? ProcessEmbedCode(string? embedCode, AudioSourceType sourceType)
{
    if (string.IsNullOrEmpty(embedCode)) return null;
    embedCode = embedCode.Trim();

    // Nếu đã là iframe, trả về nguyên bản
    if (embedCode.Contains("<iframe")) return embedCode;

    // Chuyển đổi URL thành embed format
    switch (sourceType)
    {
        case AudioSourceType.ZingMp3:
            var zingMatch = Regex.Match(embedCode, @"/([A-Z0-9]+)(?:\.html)?(?:\?|$)");
            if (zingMatch.Success)
            {
                var songId = zingMatch.Groups[1].Value;
                return $"<iframe scrolling=\"no\" width=\"100%\" height=\"80\" " +
                       $"src=\"https://zingmp3.vn/embed/song/{songId}?start=false\" " +
                       $"frameborder=\"0\" allowfullscreen=\"true\"></iframe>";
            }
            break;

        case AudioSourceType.SoundCloud:
            return $"<iframe width=\"100%\" height=\"80\" scrolling=\"no\" " +
                   $"frameborder=\"no\" src=\"https://w.soundcloud.com/player/?" +
                   $"url={Uri.EscapeDataString(embedCode)}&color=%231f6feb\"></iframe>";

        case AudioSourceType.Spotify:
            if (embedCode.Contains("spotify.com/track/"))
            {
                var trackId = embedCode.Split("/track/").Last().Split("?").First();
                return $"<iframe src=\"https://open.spotify.com/embed/track/{trackId}\" " +
                       $"width=\"100%\" height=\"80\" frameBorder=\"0\"></iframe>";
            }
            break;
    }

    return embedCode;
}
```

---

## 🎨 CSS Styling

#### File: `wwwroot/css/components.css`

```css
/* Embed Player Bar */
.player-bar-embed {
    grid-template-columns: 200px 1fr auto;
    gap: 16px;
}

.player-embed-info {
    display: flex;
    align-items: center;
    gap: 12px;
}

.player-embed-container {
    flex: 1;
    height: 80px;
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;
    border-radius: var(--ml-radius);
}

.player-embed-container iframe {
    width: 100%;
    height: 80px;
    border: none;
    border-radius: var(--ml-radius);
}

.player-embed-actions {
    display: flex;
    align-items: center;
    gap: 8px;
}

/* Badge cho bài embed */
.song-badge.embed {
    background: var(--ml-info);
    color: #fff;
    bottom: 8px;
    right: 8px;
}
```

---

## 📐 Database Schema

### Bảng Songs (sau khi thêm tính năng)

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Title | nvarchar(200) | Tên bài hát |
| AudioUrl | nvarchar(max) | Đường dẫn file MP3 (nullable) |
| **EmbedCode** | nvarchar(500) | Mã nhúng iframe (nullable) |
| **SourceType** | int | 0=Upload, 1=ZingMP3, 2=SoundCloud, 3=Spotify, 4=External |
| CoverImageUrl | nvarchar(max) | Ảnh bìa |
| Duration | int | Thời lượng (giây) |
| ... | ... | ... |

### Migration

```bash
# Xóa migration cũ và database (nếu cần)
Drop-Database

# Tạo migration mới
Add-Migration InitialCreate

# Cập nhật database
Update-Database
```

---

## 🎯 Các Platform Được Hỗ Trợ

### 1. Zing MP3

**URL Format:**
```
https://zingmp3.vn/bai-hat/Ten-Bai-Hat/ABC123.html
https://zingmp3.vn/embed/song/ABC123
```

**Embed Format:**
```html
<iframe scrolling="no" width="100%" height="80" 
    src="https://zingmp3.vn/embed/song/ABC123?start=false" 
    frameborder="0" allowfullscreen="true">
</iframe>
```

### 2. SoundCloud

**URL Format:**
```
https://soundcloud.com/artist-name/track-name
```

**Embed Format:**
```html
<iframe width="100%" height="80" scrolling="no" frameborder="no" 
    src="https://w.soundcloud.com/player/?url=https%3A//soundcloud.com/artist-name/track-name&color=%231f6feb">
</iframe>
```

### 3. Spotify

**URL Format:**
```
https://open.spotify.com/track/TRACKID
```

**Embed Format:**
```html
<iframe src="https://open.spotify.com/embed/track/TRACKID" 
    width="100%" height="80" frameBorder="0">
</iframe>
```

---

## 🖼️ UI/UX Design

### Player Bar States

#### 1. Standard Player (MP3)
```
┌─────────────────────────────────────────────────────────────────────────────┐
│ [Cover] Title   │  ⏮  ▶️  ⏭  │  0:00 ━━━━━━━━━━━━━━━ 3:45  │  🔊 ━━━  │ ☰ │
│         Artist  │   🔀    🔁   │                               │          │   │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### 2. Embed Player (Zing MP3, Spotify...)
```
┌─────────────────────────────────────────────────────────────────────────────┐
│ [Cover] Title   │        [═══ Iframe Player từ Zing/Spotify ═══]        │ ✕ │
│         Artist  │                                                        │   │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Song Card với Badge

```
┌─────────────────┐
│                 │
│  [Cover Image]  │ 🔗 ← Badge "embed" (góc dưới phải)
│      ▶️        │ ⭐ ← Badge "featured" (góc trên phải)
│                 │
├─────────────────┤
│ Song Title      │
│ Artist Name     │
│ 👁 1M  ⏱ 3:45   │
│ ♡  +  •••      │
└─────────────────┘
```

---

## 🔒 Bảo mật

### Sanitization
- EmbedCode được lưu trữ và render bằng `@Html.Raw()`, cần đảm bảo:
  - Chỉ Admin có quyền nhập EmbedCode
  - Validate format iframe trước khi lưu
  - Whitelist các domain được phép (zingmp3.vn, soundcloud.com, spotify.com)

### Potential Improvement
```csharp
private bool IsValidEmbedSource(string embedCode)
{
    var allowedDomains = new[] {
        "zingmp3.vn",
        "soundcloud.com",
        "w.soundcloud.com",
        "open.spotify.com"
    };
    
    var srcMatch = Regex.Match(embedCode, @"src=[""']([^""']+)[""']");
    if (srcMatch.Success)
    {
        var src = srcMatch.Groups[1].Value;
        return allowedDomains.Any(d => src.Contains(d));
    }
    
    return false;
}
```

---

## 📁 Files Changed

| File | Thay đổi |
|------|----------|
| `Models/Song.cs` | Thêm `EmbedCode`, `SourceType`, `AudioSourceType` enum |
| `Models/ViewModels/AdminViewModels.cs` | Thêm fields vào `AdminSongFormViewModel` |
| `Areas/Admin/Controllers/SongsController.cs` | Xử lý embed trong Create, thêm `ProcessEmbedCode()` |
| `Controllers/SongsController.cs` | API `GetSongData` trả về `sourceType`, `embedCode` |
| `Areas/Admin/Views/Songs/Create.cshtml` | Form UI chọn nguồn nhạc |
| `Views/Shared/_Player.cshtml` | Thêm Embed Player Bar |
| `Views/Songs/Details.cshtml` | Hiển thị embed player trong trang chi tiết |
| `Views/Home/Index.cshtml` | Badge cho bài embed |
| `wwwroot/js/player.js` | Logic `playEmbedSong()`, `closeEmbedPlayer()` |
| `wwwroot/css/components.css` | Styles cho `.player-bar-embed` |
| `wwwroot/css/pages.css` | Styles cho `.song-badge.embed` |

---

## ✅ Testing Checklist

- [ ] Tạo bài hát bằng Upload file MP3
- [ ] Tạo bài hát bằng URL Zing MP3
- [ ] Tạo bài hát bằng mã iframe Zing MP3
- [ ] Tạo bài hát bằng URL SoundCloud
- [ ] Tạo bài hát bằng URL Spotify
- [ ] Click Play bài Upload → Phát trong standard player bar
- [ ] Click Play bài Embed → Hiển thị iframe trong embed player bar
- [ ] Click ✕ trên embed player → Quay lại standard player bar
- [ ] Xem chi tiết bài hát embed → Hiển thị iframe trong trang
- [ ] Badge "embed" hiển thị đúng trên song card
- [ ] Edit bài hát - giữ nguyên SourceType và EmbedCode

---

## 🚀 Future Improvements

1. **Auto-fetch metadata** - Tự động lấy thông tin bài hát từ API
2. **Thumbnail extraction** - Tự động lấy ảnh bìa từ embed
3. **Duration detection** - Tự động lấy thời lượng
4. **YouTube Music support** - Thêm hỗ trợ YouTube Music
5. **Apple Music support** - Thêm hỗ trợ Apple Music
6. **Embed validation** - Validate iframe trước khi lưu
7. **Mini player mode** - Thu nhỏ embed player khi scroll
8. **Queue support for embed** - Hỗ trợ queue cho bài embed

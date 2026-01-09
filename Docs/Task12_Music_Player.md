# Task 12: Music Player - Chi tiết triển khai

## 📋 Tổng quan

Task 12 triển khai hệ thống Music Player đầy đủ chức năng, bao gồm phát nhạc, quản lý hàng đợi (queue), điều khiển volume, và các tính năng shuffle/repeat.

## 📁 Các file được tạo/cập nhật

### JavaScript

#### wwwroot/js/player.js
**Đường dẫn:** `wwwroot/js/player.js`

**Class MusicPlayer:**

```javascript
class MusicPlayer {
    constructor() {
        this.audio = document.getElementById('audioPlayer');
        this.isPlaying = false;
        this.isShuffle = false;
        this.repeatMode = 0; // 0: no, 1: all, 2: one
        this.currentIndex = 0;
        this.queue = [];
        this.volume = 1;
    }
}
```

**Các method chính:**

| Method | Mô tả |
|--------|-------|
| `playSong(song)` | Phát một bài hát với data object |
| `addToQueue(song)` | Thêm bài vào hàng đợi |
| `playFromQueue(index)` | Phát bài tại vị trí index |
| `removeFromQueue(index)` | Xóa bài khỏi hàng đợi |
| `clearQueue()` | Xóa toàn bộ hàng đợi |
| `togglePlay()` | Play/Pause |
| `playPrevious()` | Bài trước |
| `playNext()` | Bài tiếp theo |
| `toggleShuffle()` | Bật/tắt shuffle |
| `toggleRepeat()` | Chuyển chế độ repeat |
| `toggleMute()` | Bật/tắt mute |
| `seek(e)` | Tua tới vị trí |
| `setVolume(e)` | Đặt âm lượng |

**Global Functions:**

```javascript
// Phát bài hát từ ID
async function playSong(songId) {
    const response = await fetch(`/Songs/GetSongData/${songId}`);
    const songData = await response.json();
    player.playSong(songData);
}

// Phát album từ ID  
async function playAlbum(albumId, startIndex = 0) {
    const response = await fetch(`/Albums/GetAlbumSongs/${albumId}`);
    const songs = await response.json();
    // Load queue và phát
}

// Phát playlist từ ID
async function playPlaylist(playlistId, startIndex = 0) {
    const response = await fetch(`/Playlists/GetPlaylistSongs/${playlistId}`);
    const songs = await response.json();
    // Load queue và phát
}

// Thêm vào hàng đợi
async function addToQueue(songId) { ... }

// Toggle like
async function toggleLike(songId) { ... }

// Thêm vào playlist
function addToPlaylist(songId) { ... }
```

### Partial Views

#### Views/Shared/_Player.cshtml
**Layout Player Bar:**

```
┌─────────────────────────────────────────────────────────────────┐
│ [Cover] Title    │ ⏮ ◀ ▶ ▶ ⏭    │ 0:00 ═══●═══ 3:45  │ 🔊━━━  │
│          Artist  │   🔀    🔁    │                    │ 📋 📝  │
└─────────────────────────────────────────────────────────────────┘
```

**Sections:**
1. **Now Playing**: Cover image, title, artist, like/add buttons
2. **Controls**: Shuffle, prev, play/pause, next, repeat
3. **Progress**: Current time, seekbar, total time
4. **Volume**: Mute button, volume slider, queue/lyrics buttons

#### Views/Shared/_AddToPlaylistModal.cshtml
**Chức năng:**
- Modal hiển thị danh sách playlist của user
- Click để thêm bài vào playlist
- Link tạo playlist mới

### Controllers Updates

#### PlaylistsController.cs - Thêm GetPlaylistSongs
```csharp
[HttpGet]
public async Task<IActionResult> GetPlaylistSongs(int id)
{
    var playlist = await _context.Playlists
        .Include(p => p.PlaylistSongs.OrderBy(ps => ps.Order))
            .ThenInclude(ps => ps.Song.Artist)
        .FirstOrDefaultAsync(p => p.Id == id && (p.IsPublic || p.UserId == currentUserId));

    var songs = playlist.PlaylistSongs.Select(ps => new {
        id = ps.Song.Id,
        title = ps.Song.Title,
        artistName = ps.Song.Artist?.Name,
        audioUrl = ps.Song.AudioFileUrl,
        coverImageUrl = ps.Song.CoverImageUrl,
        duration = ps.Song.Duration
    });

    return Json(songs);
}
```

## 🎨 CSS Components

### Player Bar Styles (components.css)
```css
/* Player Bar */
.player-bar {
    position: fixed;
    bottom: 0;
    height: var(--ml-player-height); /* 90px */
    background: var(--ml-surface);
    display: grid;
    grid-template-columns: 1fr 2fr 1fr;
    z-index: 1000;
}

/* Progress Bar */
.progress-bar-container {
    flex: 1;
    height: 4px;
    background: var(--ml-border);
    border-radius: 2px;
    cursor: pointer;
}

.progress-bar-fill {
    height: 100%;
    background: var(--ml-primary);
    border-radius: 2px;
    transition: width 0.1s linear;
}

/* Volume Slider */
.volume-slider {
    width: 100px;
    height: 4px;
    background: var(--ml-border);
    cursor: pointer;
}

/* Play Button */
.player-btn-main {
    width: 40px;
    height: 40px;
    background: var(--ml-primary);
    border-radius: 50%;
}
```

### Queue Panel Styles
```css
.queue-panel {
    position: fixed;
    right: 0;
    top: 0;
    bottom: var(--ml-player-height);
    width: 360px;
    transform: translateX(100%);
    transition: var(--ml-transition-slow);
}

.queue-panel.show {
    transform: translateX(0);
}

.queue-item {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 12px;
    cursor: pointer;
}

.queue-item.active {
    background: rgba(var(--ml-primary-rgb), 0.15);
    border-left: 3px solid var(--ml-primary);
}

.queue-item-playing {
    animation: pulse 1.5s ease-in-out infinite;
}
```

## 🔗 API Endpoints cho Player

| Method | Route | Response | Mô tả |
|--------|-------|----------|-------|
| GET | `/Songs/GetSongData/{id}` | JSON | Lấy data bài hát |
| GET | `/Albums/GetAlbumSongs/{id}` | JSON Array | Lấy danh sách bài trong album |
| GET | `/Playlists/GetPlaylistSongs/{id}` | JSON Array | Lấy danh sách bài trong playlist |
| POST | `/Songs/Like/{id}` | JSON | Toggle like bài hát |
| GET | `/Playlists/GetUserPlaylists` | JSON Array | Lấy playlist của user |
| POST | `/Playlists/AddSong/{playlistId}/{songId}` | JSON | Thêm bài vào playlist |

### Response Format

**Song Data:**
```json
{
    "id": 1,
    "title": "Tên bài hát",
    "artistName": "Tên nghệ sĩ",
    "audioUrl": "/uploads/songs/file.mp3",
    "coverImageUrl": "/uploads/covers/cover.jpg",
    "duration": 225
}
```

## ⌨️ Keyboard Shortcuts

| Phím | Chức năng |
|------|-----------|
| `Space` | Play/Pause |
| `←` | Tua lùi 5 giây |
| `→` | Tua tới 5 giây |
| `↑` | Tăng volume 10% |
| `↓` | Giảm volume 10% |
| `M` | Mute/Unmute |
| `N` | Bài tiếp theo |
| `P` | Bài trước |

## 🔄 Player State Management

### LocalStorage Keys
```javascript
localStorage.setItem('ml_volume', volume);      // Lưu volume
localStorage.setItem('ml_queue', JSON.stringify(queue)); // Lưu queue
```

### Audio Events
```javascript
audio.addEventListener('loadedmetadata', () => {
    // Cập nhật total time
});

audio.addEventListener('timeupdate', () => {
    // Cập nhật progress bar và current time
});

audio.addEventListener('ended', () => {
    // Xử lý kết thúc bài: repeat hoặc next
});
```

## 🎵 Repeat Modes

| Mode | Giá trị | Icon | Mô tả |
|------|---------|------|-------|
| None | 0 | bi-repeat | Không lặp |
| All | 1 | bi-repeat (active) | Lặp tất cả queue |
| One | 2 | bi-repeat-1 | Lặp 1 bài |

## 📱 Responsive Design

### Desktop (>991px)
- Full player bar với 3 columns
- Queue panel 360px

### Tablet (768-991px)
- Giảm padding
- Queue panel full height

### Mobile (<768px)
- Compact player bar
- Queue panel full screen
- Ẩn một số controls

## 🧪 Test Scenarios

### Player Controls
1. ✅ Click Play → Audio plays, icon chuyển Pause
2. ✅ Click Pause → Audio pauses, icon chuyển Play
3. ✅ Click Next → Chuyển bài tiếp
4. ✅ Click Prev (< 3s) → Chuyển bài trước
5. ✅ Click Prev (> 3s) → Restart bài hiện tại
6. ✅ Toggle Shuffle → Icon active/inactive
7. ✅ Toggle Repeat → Cycle qua 3 modes

### Progress & Volume
1. ✅ Click progress bar → Tua tới vị trí
2. ✅ Drag volume slider → Thay đổi volume
3. ✅ Click mute → Mute/unmute
4. ✅ Time update → Progress bar cập nhật

### Queue Management
1. ✅ Play song → Thêm vào queue
2. ✅ Play album → Load tất cả bài vào queue
3. ✅ Add to queue → Thêm vào cuối queue
4. ✅ Remove from queue → Xóa khỏi queue
5. ✅ Click queue item → Phát bài đó

### Integration
1. ✅ Click play trên song card → Phát bài
2. ✅ Click play trên album → Phát album
3. ✅ Click play trên playlist → Phát playlist
4. ✅ Add to playlist modal → Hiện danh sách playlist

## 📦 Dependencies

- HTML5 Audio API
- Bootstrap 5 Modal
- Bootstrap Icons
- LocalStorage API

## 🔧 Troubleshooting

### Audio không phát
1. Kiểm tra file path có đúng
2. Kiểm tra browser hỗ trợ format
3. Kiểm tra CORS nếu cross-origin

### Queue không lưu
1. Kiểm tra localStorage available
2. Kiểm tra JSON stringify/parse

### Progress bar không cập nhật
1. Kiểm tra audio duration > 0
2. Kiểm tra timeupdate event listener

## 📈 Improvements có thể thêm

1. **Waveform visualization**: Hiển thị sóng âm
2. **Equalizer**: Điều chỉnh EQ
3. **Crossfade**: Fade chuyển bài
4. **Mini player**: Player thu nhỏ
5. **Picture-in-picture**: Video mode
6. **Chromecast**: Cast to TV
7. **Download offline**: Download nhạc offline
8. **Lyrics sync**: Đồng bộ lời bài hát
9. **Sleep timer**: Hẹn giờ tắt
10. **Playback speed**: Thay đổi tốc độ phát

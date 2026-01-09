# Task 17: Player Lyrics Panel

## Mục tiêu
Thêm tính năng xem lyrics trực tiếp từ music player bar khi đang phát nhạc.

## Yêu cầu
- Hiển thị panel lyrics khi click vào button lyrics ở góc phải player
- Panel slide ra từ bên phải màn hình
- Hiển thị thông tin bài hát (ảnh bìa, tên, nghệ sĩ)
- Hiển thị nội dung lyrics
- Tự động cập nhật khi chuyển bài
- Hỗ trợ keyboard shortcut

## Thiết kế UI

### Lyrics Panel Structure
```
┌─────────────────────────────────────┐
│  Lyrics                         [X] │  <- Header với nút đóng
├─────────────────────────────────────┤
│  [Cover]  Song Title                │  <- Thông tin bài hát
│           Artist Name               │
├─────────────────────────────────────┤
│                                     │
│  Lyrics content here...             │  <- Nội dung lyrics
│  Line 1                             │
│  Line 2                             │
│  Line 3                             │
│  ...                                │
│                                     │
└─────────────────────────────────────┘
```

### Player Bar Button
```
[🔊] ────────── [≡] [📄]
 Volume         Queue Lyrics
```

## Các file cần chỉnh sửa

### 1. Views/Shared/_Player.cshtml
Thêm Lyrics Panel HTML sau Queue Panel:

```html
<!-- Lyrics Panel -->
<div class="lyrics-panel" id="lyricsPanel">
    <div class="lyrics-panel-header">
        <h5 class="lyrics-panel-title">
            <i class="bi bi-music-note-list me-2"></i>Lyrics
        </h5>
        <button class="btn-icon" id="btnCloseLyrics">
            <i class="bi bi-x-lg"></i>
        </button>
    </div>
    <div class="lyrics-panel-song">
        <img src="/images/default-song.svg" alt="Cover" class="lyrics-panel-cover" id="lyricsCover" />
        <div class="lyrics-panel-info">
            <div class="lyrics-panel-song-title" id="lyricsSongTitle">Select a song</div>
            <div class="lyrics-panel-song-artist" id="lyricsSongArtist">---</div>
        </div>
    </div>
    <div class="lyrics-panel-content" id="lyricsContent">
        <div class="empty-state">
            <i class="bi bi-file-text"></i>
            <p class="text-muted">No lyrics available</p>
        </div>
    </div>
</div>
```

### 2. wwwroot/css/components.css
Thêm CSS cho Lyrics Panel:

```css
/* ============================================
   LYRICS PANEL
   ============================================ */
.lyrics-panel {
    position: fixed;
    right: 0;
    top: 0;
    bottom: var(--ml-player-height);
    width: 400px;
    background: var(--ml-surface);
    border-left: 1px solid var(--ml-border);
    transform: translateX(100%);
    transition: var(--ml-transition-slow);
    z-index: 99;
    display: flex;
    flex-direction: column;
}

.lyrics-panel.show {
    transform: translateX(0);
}

.lyrics-panel-header {
    padding: 20px;
    border-bottom: 1px solid var(--ml-border);
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.lyrics-panel-title {
    font-weight: 600;
    color: var(--ml-text);
    margin: 0;
    display: flex;
    align-items: center;
}

.lyrics-panel-song {
    padding: 16px 20px;
    display: flex;
    align-items: center;
    gap: 12px;
    background: var(--ml-surface-hover);
    border-bottom: 1px solid var(--ml-border);
}

.lyrics-panel-cover {
    width: 56px;
    height: 56px;
    border-radius: var(--ml-radius);
    object-fit: cover;
}

.lyrics-panel-info {
    flex: 1;
    min-width: 0;
}

.lyrics-panel-song-title {
    font-weight: 600;
    color: var(--ml-text);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.lyrics-panel-song-artist {
    font-size: 0.875rem;
    color: var(--ml-text-muted);
}

.lyrics-panel-content {
    flex: 1;
    overflow-y: auto;
    padding: 20px;
    line-height: 2;
    color: var(--ml-text-muted);
    font-size: 1rem;
}

.lyrics-panel-content .lyrics-text {
    white-space: pre-wrap;
    color: var(--ml-text);
}

.lyrics-panel-content .empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 100%;
    text-align: center;
}

.lyrics-panel-content .empty-state i {
    font-size: 3rem;
    color: var(--ml-text-muted);
    margin-bottom: 16px;
}

/* Responsive */
@media (max-width: 767.98px) {
    .lyrics-panel {
        width: 100%;
        left: 0;
    }
}
```

### 3. wwwroot/js/player.js

#### 3.1 Thêm biến lưu bài hát hiện tại
```javascript
class MusicPlayer {
    constructor() {
        // ... existing code ...
        this.currentSong = null; // Store current song data including lyrics
        // ...
    }
}
```

#### 3.2 Thêm elements cho lyrics panel
```javascript
initElements() {
    // ... existing elements ...
    
    // Lyrics panel elements
    this.lyricsPanel = document.getElementById('lyricsPanel');
    this.lyricsContent = document.getElementById('lyricsContent');
    this.lyricsCover = document.getElementById('lyricsCover');
    this.lyricsSongTitle = document.getElementById('lyricsSongTitle');
    this.lyricsSongArtist = document.getElementById('lyricsSongArtist');
    
    // Buttons
    this.btnLyrics = document.getElementById('btnLyrics');
    this.btnCloseLyrics = document.getElementById('btnCloseLyrics');
}
```

#### 3.3 Thêm event listeners
```javascript
initEventListeners() {
    // ... existing events ...
    this.btnLyrics?.addEventListener('click', () => this.toggleLyrics());
    this.btnCloseLyrics?.addEventListener('click', () => this.toggleLyrics());
}
```

#### 3.4 Cập nhật playSong để lưu và hiển thị lyrics
```javascript
playSong(song) {
    if (!song || !song.audioUrl) {
        console.error('Invalid song data');
        return;
    }
    
    // Store current song data
    this.currentSong = song;
    
    // Update audio source
    this.audio.src = song.audioUrl;
    
    // Update UI
    this.playerCover.src = song.coverImageUrl || '/uploads/covers/default-song.jpg';
    this.playerTitle.textContent = song.title;
    this.playerArtist.textContent = song.artistName || 'Unknown Artist';
    
    // Update lyrics panel
    this.updateLyricsPanel(song);
    
    // Start playing
    this.audio.play().catch(err => {
        console.error('Error playing audio:', err);
    });
    
    // Update play count
    this.updatePlayCount(song.id);
}
```

#### 3.5 Thêm methods toggleLyrics và updateLyricsPanel
```javascript
// Toggle lyrics panel
toggleLyrics() {
    this.lyricsPanel?.classList.toggle('show');
    // Close queue panel when opening lyrics
    if (this.lyricsPanel?.classList.contains('show')) {
        this.queuePanel?.classList.remove('show');
        this.btnQueue?.classList.remove('active');
    }
    this.btnLyrics?.classList.toggle('active', this.lyricsPanel?.classList.contains('show'));
}

// Update lyrics panel with current song data
updateLyricsPanel(song) {
    if (!song) return;
    
    // Update song info in lyrics panel
    if (this.lyricsCover) {
        this.lyricsCover.src = song.coverImageUrl || song.coverUrl || '/images/default-song.svg';
    }
    if (this.lyricsSongTitle) {
        this.lyricsSongTitle.textContent = song.title || 'Unknown';
    }
    if (this.lyricsSongArtist) {
        this.lyricsSongArtist.textContent = song.artistName || song.artist || 'Unknown Artist';
    }
    
    // Update lyrics content
    if (this.lyricsContent) {
        if (song.lyrics && song.lyrics.trim()) {
            this.lyricsContent.innerHTML = `<div class="lyrics-text">${song.lyrics.replace(/\n/g, '<br>')}</div>`;
        } else {
            this.lyricsContent.innerHTML = `
                <div class="empty-state">
                    <i class="bi bi-file-text"></i>
                    <p class="text-muted">No lyrics available for this song</p>
                </div>
            `;
        }
    }
}
```

#### 3.6 Cập nhật toggleQueue để đóng lyrics panel
```javascript
toggleQueue() {
    this.queuePanel?.classList.toggle('show');
    // Close lyrics panel when opening queue
    if (this.queuePanel?.classList.contains('show')) {
        this.lyricsPanel?.classList.remove('show');
        this.btnLyrics?.classList.remove('active');
    }
    this.btnQueue?.classList.toggle('active', this.queuePanel?.classList.contains('show'));
}
```

#### 3.7 Thêm keyboard shortcuts
```javascript
handleKeyboard(e) {
    // ... existing shortcuts ...
    switch(e.code) {
        // ... existing cases ...
        case 'KeyL':
            this.toggleLyrics();
            break;
        case 'KeyQ':
            this.toggleQueue();
            break;
    }
}
```

#### 3.8 Cập nhật global functions để include lyrics
```javascript
// Global function to play a song by ID
async function playSong(songId) {
    if (!player) return;
    
    try {
        const response = await fetch(`/Songs/GetSongData/${songId}`);
        if (response.ok) {
            const songData = await response.json();
            
            if (songData.audioUrl) {
                // Map API response to player format
                const song = {
                    id: songData.id,
                    title: songData.title,
                    artistName: songData.artist,
                    artistId: songData.artistId,
                    audioUrl: songData.audioUrl,
                    coverImageUrl: songData.coverUrl,
                    duration: songData.duration,
                    lyrics: songData.lyrics
                };
                player.playSong(song);
            } else {
                showToast('This song has no audio file', 'warning');
            }
        } else {
            showToast('Cannot play song', 'error');
        }
    } catch (err) {
        console.error('Error fetching song:', err);
        showToast('Error loading song', 'error');
    }
}
```

## Keyboard Shortcuts

| Phím | Chức năng |
|------|-----------|
| `L` | Toggle Lyrics Panel |
| `Q` | Toggle Queue Panel |
| `Space` | Play/Pause |
| `M` | Mute/Unmute |
| `N` | Next song |
| `P` | Previous song |
| `←` | Rewind 5 seconds |
| `→` | Forward 5 seconds |
| `↑` | Volume up |
| `↓` | Volume down |

## API Endpoint

### GET /Songs/GetSongData/{id}
Trả về thông tin bài hát bao gồm lyrics:

```json
{
    "id": 1,
    "title": "Song Title",
    "artist": "Artist Name",
    "artistId": 1,
    "audioUrl": "/uploads/songs/song.mp3",
    "coverUrl": "/uploads/covers/cover.jpg",
    "duration": 245,
    "lyrics": "Verse 1:\nLyrics line 1\nLyrics line 2\n\nChorus:\n..."
}
```

## Cách sử dụng

### Mở Lyrics Panel
1. **Click button**: Click vào icon 📄 ở góc phải của player bar
2. **Keyboard**: Nhấn phím `L`

### Đóng Lyrics Panel
1. Click lại button lyrics
2. Click nút X trên panel
3. Nhấn phím `L`
4. Mở Queue panel (sẽ tự động đóng Lyrics panel)

## Lưu ý
- Lyrics được lấy từ database thông qua API GetSongData
- Admin có thể thêm/sửa lyrics trong Admin Panel > Songs > Create/Edit
- Nếu bài hát không có lyrics, sẽ hiển thị thông báo "No lyrics available"
- Lyrics panel và Queue panel không thể mở cùng lúc

## Testing Checklist

- [ ] Button lyrics hiển thị đúng ở player bar
- [ ] Click button mở lyrics panel
- [ ] Click button lần nữa đóng lyrics panel
- [ ] Click nút X đóng lyrics panel
- [ ] Nhấn phím L toggle lyrics panel
- [ ] Panel hiển thị đúng thông tin bài hát
- [ ] Lyrics hiển thị đúng khi có lyrics
- [ ] Hiện "No lyrics available" khi không có lyrics
- [ ] Panel tự cập nhật khi chuyển bài
- [ ] Mở Lyrics panel tự đóng Queue panel
- [ ] Responsive trên mobile

## Tổng kết

| File | Thay đổi |
|------|----------|
| `Views/Shared/_Player.cshtml` | Thêm Lyrics Panel HTML |
| `wwwroot/css/components.css` | Thêm CSS cho `.lyrics-panel` |
| `wwwroot/js/player.js` | Thêm logic xử lý lyrics panel |

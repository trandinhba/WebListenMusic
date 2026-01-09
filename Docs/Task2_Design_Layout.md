# 📋 Task 2: Design System & Base Layout

## 🎯 Mục tiêu
Xây dựng hệ thống thiết kế (Design System) và layout cơ bản cho ứng dụng MusicListen với dark theme màu blue-black.

## ✅ Các công việc đã hoàn thành

### 1. CSS Theme (`/wwwroot/css/theme.css`)

#### Color Palette
| Variable | Color | Sử dụng |
|----------|-------|---------|
| `--ml-bg` | `#0b1220` | Background chính |
| `--ml-bg-2` | `#0f1830` | Background gradient |
| `--ml-surface` | `#111a2e` | Card, modal |
| `--ml-surface-2` | `#0d1426` | Card gradient |
| `--ml-primary` | `#1f6feb` | Buttons, links |
| `--ml-primary-2` | `#0ea5e9` | Hover, accent |
| `--ml-success` | `#22c55e` | Success states |
| `--ml-danger` | `#ef4444` | Delete, errors |
| `--ml-warning` | `#f59e0b` | Warnings |
| `--ml-text` | `#e6edf3` | Text chính |
| `--ml-text-muted` | `#9aa7b3` | Text phụ |

#### CSS Variables
- Gradients: `--ml-gradient-primary`, `--ml-gradient-bg`, `--ml-gradient-surface`
- Shadows: `--ml-shadow-sm`, `--ml-shadow`, `--ml-shadow-lg`, `--ml-shadow-glow`
- Spacing: `--ml-sidebar-width` (240px), `--ml-player-height` (90px)
- Border Radius: `--ml-radius-sm` (4px) → `--ml-radius-full` (9999px)
- Transitions: `--ml-transition`, `--ml-transition-slow`

#### Components đã style
- Typography (h1-h6, text utilities)
- Custom scrollbar
- Sidebar navigation
- Buttons (primary, secondary, outline, danger, success, icon, play)
- Cards (song-card, album-card, artist-card)
- Song list table
- Forms (inputs, selects, checkboxes)
- Modals
- Alerts & Toasts
- Badges & Tags
- Pagination
- Loading & Skeleton animations

### 2. CSS Components (`/wwwroot/css/components.css`)

#### Music Player Bar
- Now playing info (cover, title, artist)
- Player controls (play, pause, prev, next, shuffle, repeat)
- Progress bar với seek functionality
- Volume control
- Extra controls (queue, lyrics)

#### UI Components
- Horizontal scroll sections
- Grid layouts cho cards
- Stats cards (Admin dashboard)
- Data tables
- Empty state
- Hero section
- Album header
- Artist header
- Dropdown menus
- Tabs
- File upload area
- Queue panel

### 3. CSS Site (`/wwwroot/css/site.css`)

#### Custom Styles
- Auth pages (login, register)
- Form validation styles
- Song card hover effects
- Genre cards
- Playlist selection items
- Verified badge
- Premium badge
- Activity feed
- Mobile menu toggle
- Drag and drop styling
- Animation classes (fade-in, slide-up)
- Custom checkbox
- Range slider
- Lyrics display

### 4. Main Layout (`/Views/Shared/_Layout.cshtml`)

#### Structure
```html
<div class="app-container">
    <aside class="sidebar">
        <!-- Logo -->
        <!-- Navigation -->
        <!-- User menu -->
    </aside>
    
    <main class="main-content">
        @RenderBody()
    </main>
    
    <!-- Music Player -->
    @await Html.PartialAsync("_Player")
</div>
```

#### Features
- Grid-based layout với sidebar và main content
- Responsive design (sidebar thu gọn trên tablet, ẩn trên mobile)
- Active state cho navigation items
- User dropdown menu
- Bootstrap 5 integration
- Bootstrap Icons
- Inter font từ Google Fonts

### 5. Music Player (`/Views/Shared/_Player.cshtml`)

#### UI Elements
- Now playing: Cover, title, artist, like button, add to playlist
- Controls: Shuffle, previous, play/pause, next, repeat
- Progress: Current time, seekable progress bar, total time
- Volume: Mute toggle, volume slider
- Extra: Queue panel toggle, lyrics toggle

#### Hidden Elements
- Audio element cho playback
- Queue panel (slide-in từ bên phải)

### 6. Add to Playlist Modal (`/Views/Shared/_AddToPlaylistModal.cshtml`)

#### Features
- Tạo playlist mới inline
- Danh sách playlist của user
- Thêm bài hát vào playlist đã chọn

### 7. JavaScript Site (`/wwwroot/js/site.js`)

#### Functions
- `initMobileMenu()`: Toggle sidebar trên mobile
- `initToasts()`: Auto-hide toast notifications
- `showToast(message, type)`: Hiện toast thông báo
- `initPlaylistModal()`: Xử lý modal thêm vào playlist
- `loadUserPlaylists()`: Load danh sách playlist của user
- `addSongToPlaylist(playlistId)`: Thêm bài hát vào playlist
- `openAddToPlaylist(songId)`: Mở modal với song id
- `initSongCards()`: Xử lý double-click để phát
- `getSongDataFromElement(element)`: Lấy data từ element
- `initSearchBox()`: Search với debounce
- `performLiveSearch(query, input)`: Live search
- `formatDuration(seconds)`: Format thời lượng
- `formatNumber(num)`: Format số (K, M, B)
- `confirmDelete(message, callback)`: Xác nhận xóa
- `copyToClipboard(text)`: Copy vào clipboard

### 8. JavaScript Player (`/wwwroot/js/player.js`)

#### MusicPlayer Class
```javascript
class MusicPlayer {
    // Properties
    audio, isPlaying, isShuffle, repeatMode
    currentIndex, queue, volume, isMuted
    
    // Methods
    playSong(song)
    addToQueue(song)
    playFromQueue(index)
    removeFromQueue(index)
    clearQueue()
    togglePlay()
    playPrevious()
    playNext()
    toggleShuffle()
    toggleRepeat()
    toggleMute()
    toggleQueue()
    seek(e)
    setVolume(e)
    updateVolumeUI()
    updateQueueUI()
    handleKeyboard(e)
    formatTime(seconds)
    showToast(message, type)
}
```

#### Keyboard Shortcuts
| Key | Action |
|-----|--------|
| Space | Play/Pause |
| ← | Seek backward 5s |
| → | Seek forward 5s |
| ↑ | Volume up |
| ↓ | Volume down |
| M | Toggle mute |
| N | Next song |
| P | Previous song |

#### LocalStorage
- `ml_volume`: Lưu volume level
- `ml_queue`: Lưu danh sách phát

## 📁 Cấu trúc files sau Task 2

```
wwwroot/
├── css/
│   ├── theme.css        # Design system & variables
│   ├── components.css   # UI components
│   └── site.css         # Custom styles
└── js/
    ├── site.js          # Common functionality
    └── player.js        # Music player

Views/
└── Shared/
    ├── _Layout.cshtml            # Main layout
    ├── _Player.cshtml            # Music player
    └── _AddToPlaylistModal.cshtml # Playlist modal
```

## 🎨 Design Principles

1. **Dark Theme**: Màu nền tối (blue-black) để giảm mỏi mắt
2. **Consistent Spacing**: Sử dụng CSS variables cho spacing
3. **Smooth Transitions**: 0.2s - 0.3s cho các hover effects
4. **Accessible**: ARIA labels, keyboard navigation
5. **Responsive**: Mobile-first approach
6. **Modular CSS**: Tách riêng theme, components, site styles

## 🔧 Responsive Breakpoints

| Breakpoint | Sidebar | Layout |
|------------|---------|--------|
| > 992px | Full width (240px) | 2 columns |
| 768px - 991px | Collapsed (72px) | 2 columns |
| < 768px | Hidden (slide-in) | 1 column |

## ✅ Trạng thái: HOÀN THÀNH
- [x] CSS Theme với Design System
- [x] CSS Components cho UI
- [x] CSS Site cho custom styles
- [x] Main Layout với sidebar
- [x] Music Player UI
- [x] Add to Playlist Modal
- [x] JavaScript functionality
- [x] Responsive design

---
**Ngày hoàn thành:** 31/12/2024
**Task tiếp theo:** Task 3 - Admin Area Foundation

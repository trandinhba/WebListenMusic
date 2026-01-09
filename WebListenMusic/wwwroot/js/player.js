/**
 * MusicListen - Player JavaScript
 * Music Player functionality
 */

class MusicPlayer {
    constructor() {
        this.audio = document.getElementById('audioPlayer');
        this.isPlaying = false;
        this.isShuffle = false;
        this.repeatMode = 0; // 0: no repeat, 1: repeat all, 2: repeat one
        this.currentIndex = 0;
        this.queue = [];
        this.volume = 1;
        this.isMuted = false;
        this.currentSong = null; // Store current song data including lyrics
        
        this.initElements();
        this.initEventListeners();
        this.loadFromStorage();
    }
    
    initElements() {
        // Player elements
        this.playerCover = document.getElementById('playerCover');
        this.playerTitle = document.getElementById('playerTitle');
        this.playerArtist = document.getElementById('playerArtist');
        this.playPauseIcon = document.getElementById('playPauseIcon');
        this.currentTimeEl = document.getElementById('currentTime');
        this.totalTimeEl = document.getElementById('totalTime');
        this.progressBar = document.getElementById('progressBar');
        this.progressFill = document.getElementById('progressFill');
        this.volumeSlider = document.getElementById('volumeSlider');
        this.volumeFill = document.getElementById('volumeFill');
        this.volumeIcon = document.getElementById('volumeIcon');
        this.queuePanel = document.getElementById('queuePanel');
        this.queueList = document.getElementById('queueList');
        
        // Lyrics panel elements
        this.lyricsPanel = document.getElementById('lyricsPanel');
        this.lyricsContent = document.getElementById('lyricsContent');
        this.lyricsCover = document.getElementById('lyricsCover');
        this.lyricsSongTitle = document.getElementById('lyricsSongTitle');
        this.lyricsSongArtist = document.getElementById('lyricsSongArtist');
        
        // Buttons
        this.btnPlayPause = document.getElementById('btnPlayPause');
        this.btnPrev = document.getElementById('btnPrev');
        this.btnNext = document.getElementById('btnNext');
        this.btnShuffle = document.getElementById('btnShuffle');
        this.btnRepeat = document.getElementById('btnRepeat');
        this.btnMute = document.getElementById('btnMute');
        this.btnQueue = document.getElementById('btnQueue');
        this.btnCloseQueue = document.getElementById('btnCloseQueue');
        this.btnLike = document.getElementById('btnLike');
        this.btnLyrics = document.getElementById('btnLyrics');
        this.btnCloseLyrics = document.getElementById('btnCloseLyrics');
    }
    
    initEventListeners() {
        // Audio events
        this.audio.addEventListener('loadedmetadata', () => this.onMetadataLoaded());
        this.audio.addEventListener('timeupdate', () => this.onTimeUpdate());
        this.audio.addEventListener('ended', () => this.onEnded());
        this.audio.addEventListener('play', () => this.onPlay());
        this.audio.addEventListener('pause', () => this.onPause());
        
        // Control buttons
        this.btnPlayPause?.addEventListener('click', () => this.togglePlay());
        this.btnPrev?.addEventListener('click', () => this.playPrevious());
        this.btnNext?.addEventListener('click', () => this.playNext());
        this.btnShuffle?.addEventListener('click', () => this.toggleShuffle());
        this.btnRepeat?.addEventListener('click', () => this.toggleRepeat());
        this.btnMute?.addEventListener('click', () => this.toggleMute());
        this.btnQueue?.addEventListener('click', () => this.toggleQueue());
        this.btnCloseQueue?.addEventListener('click', () => this.toggleQueue());
        this.btnLyrics?.addEventListener('click', () => this.toggleLyrics());
        this.btnCloseLyrics?.addEventListener('click', () => this.toggleLyrics());
        
        // Progress bar
        this.progressBar?.addEventListener('click', (e) => this.seek(e));
        
        // Volume slider
        this.volumeSlider?.addEventListener('click', (e) => this.setVolume(e));
        
        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => this.handleKeyboard(e));
    }
    
    loadFromStorage() {
        const savedVolume = localStorage.getItem('ml_volume');
        if (savedVolume) {
            this.volume = parseFloat(savedVolume);
            this.audio.volume = this.volume;
            this.updateVolumeUI();
        }
        
        const savedQueue = localStorage.getItem('ml_queue');
        if (savedQueue) {
            this.queue = JSON.parse(savedQueue);
            this.updateQueueUI();
        }
    }
    
    saveToStorage() {
        localStorage.setItem('ml_volume', this.volume.toString());
        localStorage.setItem('ml_queue', JSON.stringify(this.queue));
    }
    
    // Play a song
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
        
        // Update play count (if logged in)
        this.updatePlayCount(song.id);
    }
    
    // Add song to queue
    addToQueue(song) {
        if (!song) return;
        
        this.queue.push(song);
        this.saveToStorage();
        this.updateQueueUI();
        this.showToast('Added to queue');
    }
    
    // Play from queue
    playFromQueue(index) {
        if (index < 0 || index >= this.queue.length) return;
        
        this.currentIndex = index;
        this.playSong(this.queue[index]);
        this.updateQueueUI();
    }
    
    // Remove from queue
    removeFromQueue(index) {
        if (index < 0 || index >= this.queue.length) return;
        
        this.queue.splice(index, 1);
        if (this.currentIndex >= index && this.currentIndex > 0) {
            this.currentIndex--;
        }
        this.saveToStorage();
        this.updateQueueUI();
    }
    
    // Clear queue
    clearQueue() {
        this.queue = [];
        this.currentIndex = 0;
        this.saveToStorage();
        this.updateQueueUI();
    }
    
    // Toggle play/pause
    togglePlay() {
        if (this.isPlaying) {
            this.audio.pause();
        } else {
            this.audio.play().catch(err => console.error('Error:', err));
        }
    }
    
    // Play previous song
    playPrevious() {
        if (this.audio.currentTime > 3) {
            this.audio.currentTime = 0;
            return;
        }
        
        if (this.queue.length === 0) return;
        
        if (this.isShuffle) {
            this.currentIndex = Math.floor(Math.random() * this.queue.length);
        } else {
            this.currentIndex = this.currentIndex > 0 ? this.currentIndex - 1 : this.queue.length - 1;
        }
        
        this.playSong(this.queue[this.currentIndex]);
        this.updateQueueUI();
    }
    
    // Play next song
    playNext() {
        if (this.queue.length === 0) return;
        
        if (this.isShuffle) {
            this.currentIndex = Math.floor(Math.random() * this.queue.length);
        } else {
            this.currentIndex = (this.currentIndex + 1) % this.queue.length;
        }
        
        this.playSong(this.queue[this.currentIndex]);
        this.updateQueueUI();
    }
    
    // Toggle shuffle
    toggleShuffle() {
        this.isShuffle = !this.isShuffle;
        this.btnShuffle?.classList.toggle('active', this.isShuffle);
    }
    
    // Toggle repeat mode
    toggleRepeat() {
        this.repeatMode = (this.repeatMode + 1) % 3;
        
        const icon = this.btnRepeat?.querySelector('i');
        this.btnRepeat?.classList.remove('active');
        
        if (this.repeatMode === 1) {
            this.btnRepeat?.classList.add('active');
            icon?.classList.remove('bi-repeat-1');
            icon?.classList.add('bi-repeat');
        } else if (this.repeatMode === 2) {
            this.btnRepeat?.classList.add('active');
            icon?.classList.remove('bi-repeat');
            icon?.classList.add('bi-repeat-1');
        } else {
            icon?.classList.remove('bi-repeat-1');
            icon?.classList.add('bi-repeat');
        }
    }
    
    // Toggle mute
    toggleMute() {
        this.isMuted = !this.isMuted;
        this.audio.muted = this.isMuted;
        this.updateVolumeIcon();
    }
    
    // Toggle queue panel
    toggleQueue() {
        this.queuePanel?.classList.toggle('show');
        // Close lyrics panel when opening queue
        if (this.queuePanel?.classList.contains('show')) {
            this.lyricsPanel?.classList.remove('show');
            this.btnLyrics?.classList.remove('active');
        }
        this.btnQueue?.classList.toggle('active', this.queuePanel?.classList.contains('show'));
    }
    
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
    
    // Seek to position
    seek(e) {
        const rect = this.progressBar.getBoundingClientRect();
        const percent = (e.clientX - rect.left) / rect.width;
        this.audio.currentTime = percent * this.audio.duration;
    }
    
    // Set volume
    setVolume(e) {
        const rect = this.volumeSlider.getBoundingClientRect();
        this.volume = Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width));
        this.audio.volume = this.volume;
        this.isMuted = false;
        this.audio.muted = false;
        this.updateVolumeUI();
        this.saveToStorage();
    }
    
    // Update volume UI
    updateVolumeUI() {
        if (this.volumeFill) {
            this.volumeFill.style.width = `${this.volume * 100}%`;
        }
        this.updateVolumeIcon();
    }
    
    // Update volume icon
    updateVolumeIcon() {
        if (!this.volumeIcon) return;
        
        this.volumeIcon.className = 'bi';
        
        if (this.isMuted || this.volume === 0) {
            this.volumeIcon.classList.add('bi-volume-mute-fill');
        } else if (this.volume < 0.5) {
            this.volumeIcon.classList.add('bi-volume-down-fill');
        } else {
            this.volumeIcon.classList.add('bi-volume-up-fill');
        }
    }
    
    // Event handlers
    onMetadataLoaded() {
        this.totalTimeEl.textContent = this.formatTime(this.audio.duration);
    }
    
    onTimeUpdate() {
        const current = this.audio.currentTime;
        const duration = this.audio.duration;
        
        this.currentTimeEl.textContent = this.formatTime(current);
        
        if (duration > 0) {
            const percent = (current / duration) * 100;
            this.progressFill.style.width = `${percent}%`;
        }
    }
    
    onEnded() {
        if (this.repeatMode === 2) {
            // Repeat one
            this.audio.currentTime = 0;
            this.audio.play();
        } else if (this.queue.length > 0) {
            if (this.repeatMode === 1 || this.currentIndex < this.queue.length - 1) {
                // Repeat all or has more songs
                this.playNext();
            }
        }
    }
    
    onPlay() {
        this.isPlaying = true;
        this.playPauseIcon?.classList.remove('bi-play-fill');
        this.playPauseIcon?.classList.add('bi-pause-fill');
    }
    
    onPause() {
        this.isPlaying = false;
        this.playPauseIcon?.classList.remove('bi-pause-fill');
        this.playPauseIcon?.classList.add('bi-play-fill');
    }
    
    // Update queue UI
    updateQueueUI() {
        if (!this.queueList) return;
        
        if (this.queue.length === 0) {
            this.queueList.innerHTML = `
                <div class="empty-state">
                    <i class="bi bi-music-note-beamed"></i>
                    <p class="text-muted">No songs in queue</p>
                </div>
            `;
            return;
        }
        
        this.queueList.innerHTML = this.queue.map((song, index) => `
            <div class="queue-item ${index === this.currentIndex ? 'playing' : ''}" data-index="${index}">
                <img src="${song.coverImageUrl || '/uploads/covers/default-song.jpg'}" alt="Cover" class="queue-item-cover" />
                <div class="queue-item-info">
                    <div class="queue-item-title">${song.title}</div>
                    <div class="queue-item-artist">${song.artistName || 'Unknown Artist'}</div>
                </div>
                <button class="btn-icon" onclick="player.removeFromQueue(${index})">
                    <i class="bi bi-x"></i>
                </button>
            </div>
        `).join('');
        
        // Add click handlers
        this.queueList.querySelectorAll('.queue-item').forEach((item) => {
            item.addEventListener('click', (e) => {
                if (!e.target.closest('.btn-icon')) {
                    const index = parseInt(item.dataset.index);
                    this.playFromQueue(index);
                }
            });
        });
    }
    
    // Handle keyboard shortcuts
    handleKeyboard(e) {
        // Don't trigger if typing in input
        if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') return;
        
        switch(e.code) {
            case 'Space':
                e.preventDefault();
                this.togglePlay();
                break;
            case 'ArrowLeft':
                this.audio.currentTime = Math.max(0, this.audio.currentTime - 5);
                break;
            case 'ArrowRight':
                this.audio.currentTime = Math.min(this.audio.duration, this.audio.currentTime + 5);
                break;
            case 'ArrowUp':
                this.volume = Math.min(1, this.volume + 0.1);
                this.audio.volume = this.volume;
                this.updateVolumeUI();
                break;
            case 'ArrowDown':
                this.volume = Math.max(0, this.volume - 0.1);
                this.audio.volume = this.volume;
                this.updateVolumeUI();
                break;
            case 'KeyM':
                this.toggleMute();
                break;
            case 'KeyN':
                this.playNext();
                break;
            case 'KeyP':
                this.playPrevious();
                break;
            case 'KeyL':
                this.toggleLyrics();
                break;
            case 'KeyQ':
                this.toggleQueue();
                break;
        }
    }
    
    // Update play count
    async updatePlayCount(songId) {
        if (!songId) return;
        
        try {
            await fetch(`/api/songs/${songId}/play`, { method: 'POST' });
        } catch (err) {
            console.error('Error updating play count:', err);
        }
    }
    
    // Format time
    formatTime(seconds) {
        if (isNaN(seconds)) return '0:00';
        
        const mins = Math.floor(seconds / 60);
        const secs = Math.floor(seconds % 60);
        return `${mins}:${secs.toString().padStart(2, '0')}`;
    }
    
    // Show toast notification
    showToast(message, type = 'info') {
        const toastContainer = document.getElementById('toastContainer');
        if (!toastContainer) return;
        
        const toast = document.createElement('div');
        toast.className = 'toast show';
        toast.innerHTML = `
            <div class="toast-header">
                <i class="bi bi-music-note me-2"></i>
                <strong class="me-auto">MusicListen</strong>
                <button type="button" class="btn-close" data-bs-dismiss="toast"></button>
            </div>
            <div class="toast-body">${message}</div>
        `;
        
        toastContainer.appendChild(toast);
        
        setTimeout(() => {
            toast.remove();
        }, 3000);
    }
}

// Initialize player
let player;
document.addEventListener('DOMContentLoaded', () => {
    player = new MusicPlayer();
});

// Global function to play a song by ID
async function playSong(songId) {
    if (!player) return;
    
    try {
        const response = await fetch(`/Songs/GetSongData/${songId}`);
        if (response.ok) {
            const songData = await response.json();
            
            // Nếu là bài upload, phát bình thường
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

// Global function to play an album by ID
async function playAlbum(albumId, startIndex = 0) {
    if (!player) return;
    
    try {
        const response = await fetch(`/Albums/GetAlbumSongs/${albumId}`);
        if (response.ok) {
            const songs = await response.json();
            if (songs.length > 0) {
                player.clearQueue();
                songs.forEach(song => player.queue.push(song));
                player.currentIndex = startIndex;
                player.playSong(player.queue[startIndex]);
                player.updateQueueUI();
                player.saveToStorage();
            } else {
                showToast('Album has no songs', 'warning');
            }
        }
    } catch (err) {
        console.error('Error fetching album:', err);
        showToast('Error loading album', 'error');
    }
}

// Global function to play a playlist by ID
async function playPlaylist(playlistId, startIndex = 0) {
    if (!player) return;
    
    try {
        const response = await fetch(`/Playlists/GetPlaylistSongs/${playlistId}`);
        if (response.ok) {
            const songs = await response.json();
            if (songs.length > 0) {
                player.clearQueue();
                songs.forEach(song => player.queue.push(song));
                player.currentIndex = startIndex;
                player.playSong(player.queue[startIndex]);
                player.updateQueueUI();
                player.saveToStorage();
            } else {
                showToast('Playlist has no songs', 'warning');
            }
        }
    } catch (err) {
        console.error('Error fetching playlist:', err);
        showToast('Error loading playlist', 'error');
    }
}

// Global function to add song to queue by ID
async function addToQueue(songId) {
    if (!player) return;
    
    try {
        const response = await fetch(`/Songs/GetSongData/${songId}`);
        if (response.ok) {
            const songData = await response.json();
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
            player.addToQueue(song);
        }
    } catch (err) {
        console.error('Error adding to queue:', err);
        showToast('Error adding to queue', 'error');
    }
}

// Global toast function
function showToast(message, type = 'info') {
    if (player) {
        player.showToast(message, type);
    } else {
        const container = document.getElementById('toastContainer');
        if (!container) return;
        
        const iconMap = {
            'info': 'bi-info-circle',
            'success': 'bi-check-circle',
            'warning': 'bi-exclamation-triangle',
            'error': 'bi-x-circle'
        };
        
        const toast = document.createElement('div');
        toast.className = `toast show bg-${type === 'error' ? 'danger' : type}`;
        toast.innerHTML = `
            <div class="toast-header">
                <i class="bi ${iconMap[type]} me-2"></i>
                <strong class="me-auto">MusicListen</strong>
                <button type="button" class="btn-close" data-bs-dismiss="toast"></button>
            </div>
            <div class="toast-body">${message}</div>
        `;
        container.appendChild(toast);
        setTimeout(() => toast.remove(), 3000);
    }
}

// Global function to toggle like/favorite
async function toggleLike(songId) {
    try {
        const response = await fetch('/Profile/ToggleFavorite', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: `songId=${songId}`
        });
        
        if (response.ok) {
            const result = await response.json();
            if (result.success) {
                // Toggle heart icon - find in multiple possible locations
                const btns = document.querySelectorAll(`[data-song-id="${songId}"] .btn-icon i.bi-heart, [data-song-id="${songId}"] .btn-icon i.bi-heart-fill`);
                btns.forEach(icon => {
                    if (result.isFavorite) {
                        icon.classList.remove('bi-heart');
                        icon.classList.add('bi-heart-fill');
                        icon.closest('.btn-icon')?.classList.add('active');
                    } else {
                        icon.classList.remove('bi-heart-fill');
                        icon.classList.add('bi-heart');
                        icon.closest('.btn-icon')?.classList.remove('active');
                    }
                });
                showToast(result.message, 'success');
            } else {
                showToast(result.message || 'Please login to add favorites', 'warning');
            }
        }
    } catch (err) {
        console.error('Error toggling like:', err);
        showToast('Error updating favorite', 'error');
    }
}

// Global function to add to playlist modal
function addToPlaylist(songId) {
    const modal = document.getElementById('addToPlaylistModal');
    if (modal) {
        // Set song ID in hidden input
        const hiddenInput = document.getElementById('modalSongId');
        if (hiddenInput) hiddenInput.value = songId;
        
        modal.dataset.songId = songId;
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
        loadUserPlaylists(songId);
    }
}

// Load user playlists for add to playlist modal
async function loadUserPlaylists(songId) {
    const container = document.getElementById('playlistSelectList');
    if (!container) return;
    
    container.innerHTML = '<div class="text-center"><div class="spinner-border spinner-border-sm"></div></div>';
    
    try {
        const response = await fetch('/Playlists/GetUserPlaylists');
        if (response.ok) {
            const data = await response.json();
            
            if (!data.success) {
                container.innerHTML = '<p class="text-muted text-center">Please login to see your playlists</p>';
                return;
            }
            
            const playlists = data.playlists;
            if (playlists.length === 0) {
                container.innerHTML = '<p class="text-muted text-center">You don\'t have any playlists yet</p>';
            } else {
                container.innerHTML = playlists.map(p => `
                    <button class="playlist-select-item" onclick="addSongToPlaylist(${p.id}, ${songId})">
                        <i class="bi bi-music-note-list me-2"></i>
                        <span>${p.name}</span>
                        <span class="text-muted ms-auto">${p.songCount} songs</span>
                    </button>
                `).join('');
            }
        } else {
            container.innerHTML = '<p class="text-muted text-center">Please login to see your playlists</p>';
        }
    } catch (err) {
        console.error('Error loading playlists:', err);
        container.innerHTML = '<p class="text-danger">Error loading playlists</p>';
    }
}

// Add song to selected playlist
async function addSongToPlaylist(playlistId, songId) {
    try {
        const response = await fetch(`/Playlists/AddSong?playlistId=${playlistId}&songId=${songId}`, { method: 'POST' });
        if (response.ok) {
            const result = await response.json();
            if (result.success) {
                showToast('Added to playlist', 'success');
                bootstrap.Modal.getInstance(document.getElementById('addToPlaylistModal'))?.hide();
            } else {
                showToast(result.message || 'Cannot add song', 'warning');
            }
        }
    } catch (err) {
        showToast('Error adding song', 'error');
    }
}

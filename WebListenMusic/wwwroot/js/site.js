/**
 * MusicListen - Site JavaScript
 * Common functionality
 */

// DOM Ready
document.addEventListener('DOMContentLoaded', function() {
    initMobileMenu();
    initToasts();
    initPlaylistModal();
    initSongCards();
    initSearchBox();
});

// Mobile menu toggle
function initMobileMenu() {
    const menuToggle = document.querySelector('.mobile-menu-toggle');
    const sidebar = document.getElementById('sidebar');
    const overlay = document.querySelector('.sidebar-overlay');
    
    if (menuToggle) {
        menuToggle.addEventListener('click', () => {
            sidebar?.classList.toggle('show');
            overlay?.classList.toggle('show');
        });
    }
    
    overlay?.addEventListener('click', () => {
        sidebar?.classList.remove('show');
        overlay?.classList.remove('show');
    });
}

// Toast notifications
function initToasts() {
    // Auto-hide toasts after 3 seconds
    const toasts = document.querySelectorAll('.toast');
    toasts.forEach(toast => {
        setTimeout(() => {
            toast.classList.remove('show');
        }, 3000);
    });
}

// Show toast notification
function showToast(message, type = 'info') {
    const toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) return;
    
    const iconMap = {
        'success': 'bi-check-circle-fill text-success',
        'error': 'bi-exclamation-circle-fill text-danger',
        'warning': 'bi-exclamation-triangle-fill text-warning',
        'info': 'bi-info-circle-fill text-info'
    };
    
    const toast = document.createElement('div');
    toast.className = 'toast show fade-in';
    toast.innerHTML = `
        <div class="toast-header">
            <i class="bi ${iconMap[type] || iconMap.info} me-2"></i>
            <strong class="me-auto">MusicListen</strong>
            <button type="button" class="btn-close" onclick="this.closest('.toast').remove()"></button>
        </div>
        <div class="toast-body">${message}</div>
    `;
    
    toastContainer.appendChild(toast);
    
    setTimeout(() => {
        toast.classList.add('fade-out');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// Playlist modal
function initPlaylistModal() {
    const modal = document.getElementById('addToPlaylistModal');
    if (!modal) return;
    
    const btnCreate = document.getElementById('btnCreateNewPlaylist');
    const btnCancel = document.getElementById('btnCancelNewPlaylist');
    const btnSave = document.getElementById('btnSaveNewPlaylist');
    const form = document.getElementById('newPlaylistForm');
    const playlistList = document.getElementById('playlistSelectList');
    
    // Toggle new playlist form
    btnCreate?.addEventListener('click', () => {
        form.style.display = 'block';
        btnCreate.style.display = 'none';
    });
    
    btnCancel?.addEventListener('click', () => {
        form.style.display = 'none';
        btnCreate.style.display = 'block';
        document.getElementById('newPlaylistName').value = '';
    });
    
    // Create new playlist
    btnSave?.addEventListener('click', async () => {
        const name = document.getElementById('newPlaylistName').value.trim();
        if (!name) {
            showToast('Please enter playlist name', 'error');
            return;
        }
        
        try {
            const response = await fetch('/api/playlists', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name })
            });
            
            if (response.ok) {
                showToast('Playlist created successfully', 'success');
                loadUserPlaylists();
                form.style.display = 'none';
                btnCreate.style.display = 'block';
                document.getElementById('newPlaylistName').value = '';
            } else {
                showToast('Cannot create playlist', 'error');
            }
        } catch (err) {
            console.error('Error creating playlist:', err);
            showToast('An error occurred', 'error');
        }
    });
    
    // Load playlists when modal opens
    modal.addEventListener('show.bs.modal', () => {
        loadUserPlaylists();
    });
}

// Load user playlists
async function loadUserPlaylists() {
    const playlistList = document.getElementById('playlistSelectList');
    if (!playlistList) return;
    
    try {
        const response = await fetch('/api/playlists/mine');
        if (!response.ok) {
            playlistList.innerHTML = '<p class="text-muted text-center">Please login to use this feature</p>';
            return;
        }
        
        const playlists = await response.json();
        
        if (playlists.length === 0) {
            playlistList.innerHTML = '<p class="text-muted text-center">You don\'t have any playlists yet</p>';
            return;
        }
        
        playlistList.innerHTML = playlists.map(p => `
            <div class="playlist-select-item" data-playlist-id="${p.id}">
                <img src="${p.coverImageUrl || '/uploads/covers/default-playlist.jpg'}" alt="${p.name}" />
                <div class="playlist-info">
                    <div class="playlist-name">${p.name}</div>
                    <div class="playlist-count">${p.songCount || 0} songs</div>
                </div>
            </div>
        `).join('');
        
        // Add click handlers
        playlistList.querySelectorAll('.playlist-select-item').forEach(item => {
            item.addEventListener('click', () => {
                addSongToPlaylist(item.dataset.playlistId);
            });
        });
    } catch (err) {
        console.error('Error loading playlists:', err);
        playlistList.innerHTML = '<p class="text-danger text-center">Cannot load playlists</p>';
    }
}

// Add song to playlist
async function addSongToPlaylist(playlistId) {
    const songId = document.getElementById('modalSongId')?.value;
    if (!songId || !playlistId) return;
    
    try {
        const response = await fetch(`/api/playlists/${playlistId}/songs`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ songId: parseInt(songId) })
        });
        
        if (response.ok) {
            showToast('Added to playlist', 'success');
            bootstrap.Modal.getInstance(document.getElementById('addToPlaylistModal'))?.hide();
        } else {
            const error = await response.text();
            showToast(error || 'Cannot add to playlist', 'error');
        }
    } catch (err) {
        console.error('Error adding to playlist:', err);
        showToast('An error occurred', 'error');
    }
}

// Open add to playlist modal with song id
function openAddToPlaylist(songId) {
    document.getElementById('modalSongId').value = songId;
    const modal = new bootstrap.Modal(document.getElementById('addToPlaylistModal'));
    modal.show();
}

// Song cards interaction
function initSongCards() {
    document.querySelectorAll('.song-card, .song-list-item').forEach(card => {
        // Double click to play
        card.addEventListener('dblclick', () => {
            const songData = getSongDataFromElement(card);
            if (songData) {
                playSong(songData);
            }
        });
    });
}

// Get song data from element
function getSongDataFromElement(element) {
    const data = element.dataset;
    if (!data.songId) return null;
    
    return {
        id: parseInt(data.songId),
        title: data.songTitle || 'Unknown',
        artistName: data.artistName || 'Unknown Artist',
        audioUrl: data.audioUrl,
        coverImageUrl: data.coverUrl || '/uploads/covers/default-song.jpg',
        duration: parseInt(data.duration) || 0
    };
}

// Search box
function initSearchBox() {
    const searchInputs = document.querySelectorAll('.search-box input');
    let debounceTimer;
    
    searchInputs.forEach(input => {
        input.addEventListener('input', (e) => {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                const query = e.target.value.trim();
                if (query.length >= 2) {
                    // Trigger search
                    if (input.dataset.liveSearch === 'true') {
                        performLiveSearch(query, input);
                    }
                }
            }, 300);
        });
        
        // Enter key to search
        input.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                const query = e.target.value.trim();
                if (query) {
                    window.location.href = `/Search?q=${encodeURIComponent(query)}`;
                }
            }
        });
    });
}

// Live search
async function performLiveSearch(query, input) {
    const resultsContainer = input.closest('.search-box')?.querySelector('.search-results');
    if (!resultsContainer) return;
    
    try {
        const response = await fetch(`/api/search/quick?q=${encodeURIComponent(query)}`);
        if (!response.ok) return;
        
        const results = await response.json();
        
        if (results.length === 0) {
            resultsContainer.innerHTML = '<div class="p-3 text-muted">No results found</div>';
        } else {
            resultsContainer.innerHTML = results.map(r => `
                <a href="${r.url}" class="search-result-item">
                    <img src="${r.imageUrl}" alt="" />
                    <div>
                        <div class="title">${r.title}</div>
                        <div class="subtitle">${r.subtitle}</div>
                    </div>
                </a>
            `).join('');
        }
        
        resultsContainer.classList.add('show');
    } catch (err) {
        console.error('Search error:', err);
    }
}

// Format duration
function formatDuration(seconds) {
    if (!seconds || isNaN(seconds)) return '0:00';
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs.toString().padStart(2, '0')}`;
}

// Format number
function formatNumber(num) {
    if (num >= 1000000000) return (num / 1000000000).toFixed(1) + 'B';
    if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
    if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
    return num.toString();
}

// Confirm delete
function confirmDelete(message, callback) {
    if (confirm(message || 'Are you sure you want to delete?')) {
        callback();
    }
}

// Copy to clipboard
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(() => {
        showToast('Copied to clipboard', 'success');
    }).catch(() => {
        showToast('Cannot copy', 'error');
    });
}


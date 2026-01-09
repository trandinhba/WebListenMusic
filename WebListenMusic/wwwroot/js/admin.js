/**
 * MusicListen - Admin JavaScript
 * Admin area specific functionality
 */

document.addEventListener('DOMContentLoaded', function() {
    initMobileMenu();
    initFileUploads();
    initDeleteConfirmation();
    initBulkActions();
    initCharts();
});

// Mobile menu toggle
function initMobileMenu() {
    const menuToggle = document.getElementById('mobileMenuToggle');
    const sidebar = document.getElementById('adminSidebar');
    
    menuToggle?.addEventListener('click', () => {
        sidebar?.classList.toggle('show');
    });
    
    // Close sidebar when clicking outside
    document.addEventListener('click', (e) => {
        if (sidebar?.classList.contains('show') && 
            !sidebar.contains(e.target) && 
            !menuToggle?.contains(e.target)) {
            sidebar.classList.remove('show');
        }
    });
}

// File upload handling
function initFileUploads() {
    const fileAreas = document.querySelectorAll('.file-upload-area');
    
    fileAreas.forEach(area => {
        const input = area.querySelector('input[type="file"]');
        const preview = area.parentElement.querySelector('.file-preview');
        
        // Click to open file dialog
        area.addEventListener('click', () => input?.click());
        
        // Drag and drop
        area.addEventListener('dragover', (e) => {
            e.preventDefault();
            area.classList.add('dragover');
        });
        
        area.addEventListener('dragleave', () => {
            area.classList.remove('dragover');
        });
        
        area.addEventListener('drop', (e) => {
            e.preventDefault();
            area.classList.remove('dragover');
            
            const files = e.dataTransfer.files;
            if (files.length > 0 && input) {
                input.files = files;
                handleFileSelect(input, preview);
            }
        });
        
        // File input change
        input?.addEventListener('change', () => {
            handleFileSelect(input, preview);
        });
    });
}

function handleFileSelect(input, preview) {
    const file = input.files[0];
    if (!file) return;
    
    const isImage = file.type.startsWith('image/');
    const isAudio = file.type.startsWith('audio/');
    
    if (preview) {
        let previewHtml = '';
        
        if (isImage) {
            const reader = new FileReader();
            reader.onload = (e) => {
                preview.innerHTML = `
                    <img src="${e.target.result}" alt="Preview" />
                    <div class="file-info">
                        <div class="file-name">${file.name}</div>
                        <div class="file-size">${formatFileSize(file.size)}</div>
                    </div>
                    <button type="button" class="btn btn-icon" onclick="clearFileInput(this)">
                        <i class="bi bi-x-lg"></i>
                    </button>
                `;
                preview.style.display = 'flex';
            };
            reader.readAsDataURL(file);
        } else if (isAudio) {
            preview.innerHTML = `
                <div class="file-icon">
                    <i class="bi bi-music-note-beamed"></i>
                </div>
                <div class="file-info">
                    <div class="file-name">${file.name}</div>
                    <div class="file-size">${formatFileSize(file.size)}</div>
                </div>
                <button type="button" class="btn btn-icon" onclick="clearFileInput(this)">
                    <i class="bi bi-x-lg"></i>
                </button>
            `;
            preview.style.display = 'flex';
        }
    }
}

function clearFileInput(button) {
    const preview = button.closest('.file-preview');
    const container = preview.closest('.form-group');
    const input = container.querySelector('input[type="file"]');
    
    if (input) input.value = '';
    preview.style.display = 'none';
}

function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

// Delete confirmation
function initDeleteConfirmation() {
    document.querySelectorAll('[data-delete-confirm]').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const message = btn.dataset.deleteConfirm || 'Are you sure you want to delete?';
            if (!confirm(message)) {
                e.preventDefault();
            }
        });
    });
}

// Bulk actions
function initBulkActions() {
    const selectAll = document.getElementById('selectAll');
    const checkboxes = document.querySelectorAll('.item-checkbox');
    const bulkActions = document.querySelector('.bulk-actions');
    
    selectAll?.addEventListener('change', () => {
        checkboxes.forEach(cb => cb.checked = selectAll.checked);
        updateBulkActions();
    });
    
    checkboxes.forEach(cb => {
        cb.addEventListener('change', updateBulkActions);
    });
    
    function updateBulkActions() {
        const checked = document.querySelectorAll('.item-checkbox:checked');
        if (bulkActions) {
            bulkActions.style.display = checked.length > 0 ? 'flex' : 'none';
            const countEl = bulkActions.querySelector('.selected-count');
            if (countEl) countEl.textContent = checked.length;
        }
    }
}

// Initialize charts (placeholder - would use Chart.js in production)
function initCharts() {
    const chartContainer = document.getElementById('uploadsChart');
    if (!chartContainer) return;
    
    // Get data from data attributes
    const labels = JSON.parse(chartContainer.dataset.labels || '[]');
    const songData = JSON.parse(chartContainer.dataset.songs || '[]');
    const userData = JSON.parse(chartContainer.dataset.users || '[]');
    
    // Simple chart rendering (in production, use Chart.js)
    if (typeof Chart !== 'undefined') {
        new Chart(chartContainer.getContext('2d'), {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'New Songs',
                        data: songData,
                        borderColor: '#1f6feb',
                        backgroundColor: 'rgba(31, 111, 235, 0.1)',
                        fill: true,
                        tension: 0.4
                    },
                    {
                        label: 'New Users',
                        data: userData,
                        borderColor: '#22c55e',
                        backgroundColor: 'rgba(34, 197, 94, 0.1)',
                        fill: true,
                        tension: 0.4
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            color: '#9aa7b3'
                        }
                    }
                },
                scales: {
                    x: {
                        grid: {
                            color: '#21262d'
                        },
                        ticks: {
                            color: '#9aa7b3'
                        }
                    },
                    y: {
                        grid: {
                            color: '#21262d'
                        },
                        ticks: {
                            color: '#9aa7b3'
                        }
                    }
                }
            }
        });
    }
}

// Export functions for inline use
window.clearFileInput = clearFileInput;

// Notification functionality
class NotificationManager {
    constructor() {
        this.init();
        this.loadNotificationCount();
        this.setupAutoRefresh();
    }

    init() {
        const notificationBell = document.getElementById('notificationBell');
        if (notificationBell) {
            // Add click event to load notifications when dropdown is opened
            notificationBell.addEventListener('click', () => {
                console.log('Notification bell clicked');
                this.loadNotifications();
            });
        } else {
            console.warn('Notification bell element not found');
        }
    }

    async loadNotificationCount() {
        try {
            console.log('Loading notification count...');
            const response = await fetch('/api/notification/count');
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const result = await response.json();
            console.log('Notification count result:', result);
            
            if (result.success && result.count > 0) {
                this.updateNotificationBadge(result.count);
            } else {
                this.hideNotificationBadge();
            }
        } catch (error) {
            console.error('Error loading notification count:', error);
            this.hideNotificationBadge();
        }
    }

    async loadNotifications() {
        try {
            console.log('Loading notifications...');
            
            // Show loading state
            this.showLoadingState();
            
            const response = await fetch('/api/notification/history');
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const result = await response.json();
            console.log('Notification history result:', result);
            
            if (result.success) {
                this.renderNotifications(result.data || []);
                this.updateNotificationTotal(result.count || 0);
            } else {
                this.showErrorMessage(result.message || 'Gagal memuat notifikasi');
            }
        } catch (error) {
            console.error('Error loading notifications:', error);
            this.showErrorMessage('Terjadi kesalahan saat memuat notifikasi');
        }
    }

    showLoadingState() {
        const notificationList = document.getElementById('notificationList');
        if (notificationList) {
            notificationList.innerHTML = `
                <div class="notification-empty">
                    <div class="spinner-border spinner-border-sm mb-2" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <div>Memuat notifikasi...</div>
                </div>
            `;
        }
    }

    renderNotifications(notifications) {
        const notificationList = document.getElementById('notificationList');
        
        if (!notificationList) {
            console.error('Notification list element not found');
            return;
        }
        
        if (!notifications || notifications.length === 0) {
            notificationList.innerHTML = `
                <div class="notification-empty">
                    <i class="fa fa-bell-slash fa-2x mb-2"></i>
                    <div>Tidak ada notifikasi dalam 7 hari terakhir</div>
                </div>
            `;
            return;
        }

        let html = '';
        notifications.forEach(notification => {
            try {
                console.log('Processing notification:', notification);
                
                const timeAgo = this.getTimeAgo(notification.createdAt);
                const iconClass = notification.type === 'Asset In' ? 'asset-in' : 'asset-out';
                const title = notification.title || 'Item tidak diketahui';
                const description = notification.description || 'Tidak ada deskripsi';
                const icon = notification.icon || 'fa-box';
                
                html += `
                    <li>
                        <div class="notification-item d-flex">
                            <div class="notification-icon ${iconClass}">
                                <i class="fa ${icon}"></i>
                            </div>
                            <div class="notification-content">
                                <div class="notification-title" title="${title}">${title}</div>
                                <div class="notification-desc">${description}</div>
                                <div class="d-flex justify-content-between align-items-center">
                                    <small class="notification-time">${timeAgo}</small>
                                    <span class="badge notification-badge ${notification.type === 'Asset In' ? 'bg-success' : 'bg-danger'}">
                                        ${notification.type}
                                    </span>
                                </div>
                            </div>
                        </div>
                    </li>
                `;
            } catch (err) {
                console.error('Error rendering notification item:', err, notification);
            }
        });

        notificationList.innerHTML = html;
    }

    updateNotificationBadge(count) {
        const badge = document.getElementById('notificationCount');
        if (badge) {
            badge.textContent = count > 99 ? '99+' : count;
            badge.style.display = 'inline-block';
            
            // Add ring animation
            const bell = document.getElementById('notificationBell');
            if (bell) {
                bell.classList.add('bell-ring');
                setTimeout(() => {
                    bell.classList.remove('bell-ring');
                }, 800);
            }
        }
    }

    hideNotificationBadge() {
        const badge = document.getElementById('notificationCount');
        if (badge) {
            badge.style.display = 'none';
        }
    }

    updateNotificationTotal(count) {
        const totalElement = document.getElementById('notificationTotal');
        if (totalElement) {
            totalElement.textContent = `${count} item${count !== 1 ? 's' : ''}`;
        }
    }

    showErrorMessage(message) {
        const notificationList = document.getElementById('notificationList');
        if (notificationList) {
            notificationList.innerHTML = `
                <div class="notification-empty text-danger">
                    <i class="fa fa-exclamation-triangle fa-2x mb-2"></i>
                    <div>${message}</div>
                    <button class="btn btn-sm btn-outline-danger mt-2" onclick="window.notificationManager.loadNotifications()">
                        Coba Lagi
                    </button>
                </div>
            `;
        }
    }

    getTimeAgo(dateString) {
        if (!dateString) return 'Tidak diketahui';
        
        try {
            const date = new Date(dateString);
            if (isNaN(date.getTime())) {
                return 'Tidak diketahui';
            }
            
            const now = new Date();
            const diffInSeconds = Math.floor((now - date) / 1000);
            
            if (diffInSeconds < 60) {
                return 'Baru saja';
            } else if (diffInSeconds < 3600) {
                const minutes = Math.floor(diffInSeconds / 60);
                return `${minutes} menit yang lalu`;
            } else if (diffInSeconds < 86400) {
                const hours = Math.floor(diffInSeconds / 3600);
                return `${hours} jam yang lalu`;
            } else {
                const days = Math.floor(diffInSeconds / 86400);
                return `${days} hari yang lalu`;
            }
        } catch (error) {
            console.error('Error parsing date:', error, dateString);
            return 'Tidak diketahui';
        }
    }

    setupAutoRefresh() {
        // Refresh notification count every 5 minutes
        setInterval(() => {
            this.loadNotificationCount();
        }, 5 * 60 * 1000);
    }
}

// Initialize notification manager when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    console.log('Initializing notification manager...');
    window.notificationManager = new NotificationManager();
});

// Function to manually refresh notifications (can be called from other scripts)
function refreshNotifications() {
    console.log('Manual refresh notifications called');
    if (window.notificationManager) {
        window.notificationManager.loadNotificationCount();
    } else {
        console.warn('Notification manager not available');
    }
}
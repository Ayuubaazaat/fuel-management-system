// Notification Module JavaScript

document.addEventListener('DOMContentLoaded', function () {

    // Handle module filter clicks
    document.querySelectorAll('.module-filter-link').forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            const module = this.dataset.module;
            const url = new URL(window.location.href);
            if (module) {
                url.searchParams.set('module', module);
            } else {
                url.searchParams.delete('module');
            }
            url.searchParams.set('page', '1');
            window.location.href = url.toString();
        });
    });

    document.querySelectorAll('.mark-read-btn').forEach(btn => {
        btn.addEventListener('click', async function (e) {
            e.preventDefault();
            e.stopPropagation();
            await markAsRead(this.dataset.id, this);
        });
    });

    document.querySelectorAll('.delete-notification-btn').forEach(btn => {
        btn.addEventListener('click', async function (e) {
            e.preventDefault();
            e.stopPropagation();
            await deleteNotification(this.dataset.id, this);
        });
    });

    const markAllBtn = document.getElementById('markAllReadBtn');
    if (markAllBtn) {
        markAllBtn.addEventListener('click', async function (e) {
            e.preventDefault();
            await markAllAsRead();
        });
    }

    // ── Dropdown "mark all read" button (inside bell popup) ──
    const markAllDropdownBtn = document.getElementById('markAllReadDropdown');
    if (markAllDropdownBtn) {
        markAllDropdownBtn.addEventListener('click', async function (e) {
            e.preventDefault();
            e.stopPropagation();
            await markAllAsRead();
        });
    }

    const clearHistoryBtn = document.getElementById('clearHistoryBtn');
    if (clearHistoryBtn) {
        clearHistoryBtn.addEventListener('click', function (e) {
            e.preventDefault();
            openClearHistoryModal();
        });
    }

    document.querySelectorAll('.pagination-btn').forEach(btn => {
        btn.addEventListener('click', function (e) {
            if (!this.hasAttribute('disabled')) {
                const page = this.dataset.page;
                const url = new URL(window.location.href);
                url.searchParams.set('page', page);
                window.location.href = url.toString();
            }
        });
    });

    const loadMoreBtn = document.getElementById('loadMoreBtn');
    if (loadMoreBtn) {
        loadMoreBtn.addEventListener('click', async function (e) {
            e.preventDefault();
            await loadMoreNotifications();
        });
    }

    setupClearHistoryModal();

    // Auto-refresh unread count every 30 seconds
    setInterval(refreshUnreadCount, 30000);
});

function setupClearHistoryModal() {
    const modal = document.getElementById('clearHistoryModal');
    const cancelBtn = document.getElementById('cancelClearBtn');
    const confirmBtn = document.getElementById('confirmClearBtn');

    if (cancelBtn) {
        cancelBtn.addEventListener('click', function () {
            closeClearHistoryModal();
        });
    }
    if (confirmBtn) {
        confirmBtn.addEventListener('click', async function () {
            await executeClearHistory();
        });
    }
    if (modal) {
        modal.addEventListener('click', function (e) {
            if (e.target === modal) closeClearHistoryModal();
        });
    }
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && modal && !modal.classList.contains('hidden')) {
            closeClearHistoryModal();
        }
    });
}

function openClearHistoryModal() {
    const modal = document.getElementById('clearHistoryModal');
    if (modal) {
        modal.classList.remove('hidden');
        document.body.style.overflow = 'hidden';
    }
}

function closeClearHistoryModal() {
    const modal = document.getElementById('clearHistoryModal');
    if (modal) {
        modal.classList.add('hidden');
        document.body.style.overflow = '';
    }
}

async function executeClearHistory() {
    try {
        const response = await fetch('/Notification/ClearHistory', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            }
        });
        const data = await response.json();
        if (data.success) {
            closeClearHistoryModal();
            showToast('Notification history cleared successfully', 'success');
            setTimeout(() => window.location.reload(), 1500);
        }
    } catch (error) {
        console.error('Error clearing history:', error);
        showToast('Failed to clear history', 'error');
        closeClearHistoryModal();
    }
}

async function markAsRead(id, element) {
    try {
        const response = await fetch('/Notification/MarkAsRead', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({ id })
        });
        const data = await response.json();
        if (data.success) {
            const notificationItem = element.closest('.notification-item');
            if (notificationItem) {
                notificationItem.classList.remove('border-l-4', 'border-l-purple-500');
                notificationItem.dataset.status = '1';
                element.remove();
                await refreshUnreadCount();
                showToast('✓ Notification marked as read', 'info');
            }
        } else {
            showToast('Failed to mark as read', 'error');
        }
    } catch (error) {
        console.error('Error marking as read:', error);
        showToast('Failed to mark notification as read', 'error');
    }
}

async function deleteNotification(id, element) {
    const confirmed = await showDeleteConfirmationModal();
    if (!confirmed) return;

    try {
        const response = await fetch('/Notification/Delete', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({ id })
        });
        const data = await response.json();
        if (data.success) {
            const notificationItem = element.closest('.notification-item');
            notificationItem.remove();
            await refreshUnreadCount();

            const notificationsList = document.getElementById('notificationsList');
            if (notificationsList && notificationsList.children.length === 0) {
                notificationsList.innerHTML = `
                    <div class="bg-white dark:bg-gray-800 rounded-2xl p-12 text-center border border-gray-100 dark:border-gray-700">
                        <div class="w-20 h-20 bg-purple-100 dark:bg-purple-900/30 rounded-2xl flex items-center justify-center mx-auto mb-4">
                            <i class="fas fa-bell-slash text-3xl text-purple-600 dark:text-purple-400"></i>
                        </div>
                        <h3 class="text-lg font-semibold text-gray-800 dark:text-gray-100 mb-2">No notifications found</h3>
                        <p class="text-sm text-gray-500 dark:text-gray-400">Try adjusting your filters or check back later.</p>
                    </div>
                `;
            }
            showToast('Notification cleared', 'success');
        }
    } catch (error) {
        console.error('Error deleting notification:', error);
        showToast('Failed to clear notification', 'error');
    }
}

async function showDeleteConfirmationModal() {
    return new Promise((resolve) => {
        const existingModal = document.getElementById('deleteConfirmModal');
        if (existingModal) existingModal.remove();

        const modalHtml = `
            <div id="deleteConfirmModal" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
                <div class="bg-white dark:bg-gray-800 rounded-2xl shadow-2xl max-w-md w-full mx-4 p-6 animate-scaleIn">
                    <div class="flex items-center justify-center w-16 h-16 bg-yellow-100 dark:bg-yellow-900/30 rounded-2xl mx-auto mb-4">
                        <i class="fas fa-exclamation-circle text-3xl text-yellow-600 dark:text-yellow-400"></i>
                    </div>
                    <h3 class="text-xl font-bold text-gray-800 dark:text-gray-100 text-center mb-2">Clear Notification</h3>
                    <p class="text-sm text-gray-600 dark:text-gray-400 text-center mb-6">
                        Are you sure you want to clear this notification?<br>This action cannot be undone.
                    </p>
                    <div class="flex gap-3">
                        <button id="cancelDeleteBtn" class="flex-1 px-4 py-3 bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-300 font-medium rounded-xl transition-colors">
                            Cancel
                        </button>
                        <button id="confirmDeleteBtn" class="flex-1 px-4 py-3 bg-gradient-to-r from-red-600 to-red-500 hover:from-red-700 hover:to-red-600 text-white font-medium rounded-xl transition-all duration-300 transform hover:scale-105">
                            Clear
                        </button>
                    </div>
                </div>
            </div>
        `;

        const modalContainer = document.createElement('div');
        modalContainer.innerHTML = modalHtml;
        document.body.appendChild(modalContainer);
        document.body.style.overflow = 'hidden';

        const modal = document.getElementById('deleteConfirmModal');
        const cancelBtn = document.getElementById('cancelDeleteBtn');
        const confirmBtn = document.getElementById('confirmDeleteBtn');

        function cleanup() {
            if (modal && modal.parentNode) {
                modal.remove();
                document.body.style.overflow = '';
            }
        }

        cancelBtn.addEventListener('click', (e) => { e.preventDefault(); e.stopPropagation(); cleanup(); resolve(false); });
        confirmBtn.addEventListener('click', (e) => { e.preventDefault(); e.stopPropagation(); cleanup(); resolve(true); });
        modal.addEventListener('click', (e) => { if (e.target === modal) { cleanup(); resolve(false); } });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') { cleanup(); resolve(false); }
        }, { once: true });
    });
}

async function markAllAsRead() {
    try {
        const response = await fetch('/Notification/MarkAllAsRead', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            }
        });
        const data = await response.json();
        if (data.success) {
            document.querySelectorAll('.notification-item').forEach(item => {
                item.classList.remove('border-l-4', 'border-l-purple-500');
                const markReadBtn = item.querySelector('.mark-read-btn');
                if (markReadBtn) markReadBtn.remove();
            });
            await refreshUnreadCount();
            showToast('All notifications marked as read', 'success');
        }
    } catch (error) {
        console.error('Error marking all as read:', error);
        showToast('Failed to mark all as read', 'error');
    }
}

async function loadMoreNotifications() {
    const loadMoreBtn = document.getElementById('loadMoreBtn');
    const currentPage = parseInt(loadMoreBtn?.dataset.page || '1');
    const nextPage = currentPage + 1;

    try {
        loadMoreBtn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Loading...';
        loadMoreBtn.disabled = true;

        const url = new URL(window.location.href);
        url.searchParams.set('page', nextPage);
        url.searchParams.set('loadMore', 'true');

        const response = await fetch(url.toString(), {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        const html = await response.text();
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = html;

        const newNotifications = tempDiv.querySelectorAll('.notification-item');
        const notificationsList = document.getElementById('notificationsList');

        newNotifications.forEach(notification => {
            notificationsList.appendChild(notification);
            const markReadBtn = notification.querySelector('.mark-read-btn');
            if (markReadBtn) {
                markReadBtn.addEventListener('click', async function (e) {
                    e.preventDefault(); e.stopPropagation();
                    await markAsRead(this.dataset.id, this);
                });
            }
            const deleteBtn = notification.querySelector('.delete-notification-btn');
            if (deleteBtn) {
                deleteBtn.addEventListener('click', async function (e) {
                    e.preventDefault(); e.stopPropagation();
                    await deleteNotification(this.dataset.id, this);
                });
            }
        });

        const totalPages = parseInt(loadMoreBtn.dataset.totalPages || '1');
        if (nextPage >= totalPages) {
            loadMoreBtn.remove();
        } else {
            loadMoreBtn.dataset.page = nextPage;
            loadMoreBtn.innerHTML = '<i class="fas fa-sync-alt mr-2"></i>Load More Activity';
            loadMoreBtn.disabled = false;
        }
    } catch (error) {
        console.error('Error loading more notifications:', error);
        showToast('Failed to load more notifications', 'error');
        loadMoreBtn.innerHTML = '<i class="fas fa-sync-alt mr-2"></i>Load More Activity';
        loadMoreBtn.disabled = false;
    }
}

async function refreshUnreadCount() {
    try {
        const response = await fetch('/Notification/GetUnreadCount');
        const data = await response.json();

        const badge = document.getElementById('notificationBadge');
        const mobileBadge = document.getElementById('mobileNotificationBadge');
        const dropdownBadge = document.getElementById('dropdownUnreadBadge');
        const unreadSpan = document.getElementById('unreadCount');

        if (data.count > 0) {
            if (badge) { badge.textContent = data.count; badge.classList.remove('hidden'); }
            if (mobileBadge) { mobileBadge.textContent = data.count; mobileBadge.classList.remove('hidden'); }
            if (dropdownBadge) dropdownBadge.textContent = data.count + ' new';
            if (unreadSpan) unreadSpan.textContent = data.count;
        } else {
            if (badge) badge.classList.add('hidden');
            if (mobileBadge) mobileBadge.classList.add('hidden');
            if (dropdownBadge) dropdownBadge.textContent = '0 new';
            if (unreadSpan) unreadSpan.textContent = '0';
        }
    } catch (error) {
        console.error('Error refreshing unread count:', error);
    }
}

function getAntiForgeryToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : '';
}

function showToast(message, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `fixed bottom-4 right-4 px-6 py-3 rounded-xl shadow-lg text-white z-50 animate-slideIn ${type === 'success' ? 'bg-green-500' :
            type === 'error' ? 'bg-red-500' : 'bg-blue-500'
        }`;
    toast.textContent = message;
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 3000);
}
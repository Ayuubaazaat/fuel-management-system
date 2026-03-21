// Mobile notification only
document.addEventListener('DOMContentLoaded', function () {
    const mobileBtn = document.getElementById('notificationToggleMobile');
    const mobileDropdown = document.getElementById('mobileNotificationDropdown');
    const mobileContainer = document.getElementById('mobileNotificationContainer');

    if (mobileBtn && mobileDropdown) {

        // Remove 'hidden' from inner panel on load so it shows when parent is toggled
        const innerPanel = mobileDropdown.querySelector('.notificationDropdownPanel');
        if (innerPanel) innerPanel.classList.remove('hidden');

        mobileBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            const mobileProfileMenu = document.getElementById('mobileProfileMenu');
            const mobileSearchForm = document.getElementById('mobileSearchForm');
            if (mobileProfileMenu) mobileProfileMenu.classList.add('hidden');
            if (mobileSearchForm) mobileSearchForm.classList.add('hidden');
            mobileDropdown.classList.toggle('hidden');
        });

        document.addEventListener('click', function (e) {
            if (mobileContainer && !mobileContainer.contains(e.target)) {
                mobileDropdown.classList.add('hidden');
            }
        });
    }
});
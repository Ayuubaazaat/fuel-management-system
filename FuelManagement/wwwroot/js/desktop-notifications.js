// Desktop notification only
document.addEventListener('DOMContentLoaded', function () {
    const bell = document.getElementById('notificationBell');

    // CHANGED: querySelector by class since _PartialDropdown no longer has id="notificationDropdown"
    const dropdown = bell ? bell.parentElement.querySelector('.notificationDropdownPanel') : null;

    if (bell && dropdown) {
        // ADDED: assign ID at runtime so refreshUnreadCount and other functions still find it
        dropdown.id = 'notificationDropdown';

        bell.addEventListener('click', function (e) {
            e.stopPropagation();
            dropdown.classList.toggle('hidden');
        });

        document.addEventListener('click', function (e) {
            if (!bell.contains(e.target) && !dropdown.contains(e.target)) {
                dropdown.classList.add('hidden');
            }
        });
    }
});
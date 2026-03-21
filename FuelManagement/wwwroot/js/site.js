// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Theme Management
(function () {
    const html = document.documentElement;

    // Helper to set theme in both cookie and localStorage
    function setTheme(theme) {
        if (theme === 'dark') {
            html.classList.add('dark');
        } else {
            html.classList.remove('dark');
        }

        // Save to localStorage
        localStorage.setItem('theme', theme);

        // Save to cookie for cross-page persistence
        document.cookie = ".FuelMS.Theme=" + theme + "; path=/; max-age=604800"; // 7 days

        // Update icons
        updateThemeIcon();
    }

    // Initialize theme
    function initTheme() {
        console.log('Initializing theme...');

        // Check for theme cookie first
        const themeCookie = document.cookie.split('; ').find(row => row.startsWith('.FuelMS.Theme='));
        let savedTheme = null;

        if (themeCookie) {
            savedTheme = themeCookie.split('=')[1];
        } else {
            // Fallback to localStorage
            savedTheme = localStorage.getItem('theme');
        }

        const systemPrefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;

        // Determine which theme to apply
        if (savedTheme === 'dark') {
            setTheme('dark');
        } else if (savedTheme === 'light') {
            setTheme('light');
        } else {
            // No saved preference - use system preference
            if (systemPrefersDark) {
                setTheme('dark');
            } else {
                setTheme('light');
            }
        }
    }

    // Update icon based on theme
    function updateThemeIcon() {
        const icon = document.getElementById('themeIcon');
        const iconMobile = document.getElementById('themeIconMobile');

        const isDark = html.classList.contains('dark');

        if (icon) {
            if (isDark) {
                icon.classList.remove('fa-moon');
                icon.classList.add('fa-sun');
            } else {
                icon.classList.remove('fa-sun');
                icon.classList.add('fa-moon');
            }
        }

        if (iconMobile) {
            if (isDark) {
                iconMobile.classList.remove('fa-moon');
                iconMobile.classList.add('fa-sun');
            } else {
                iconMobile.classList.remove('fa-sun');
                iconMobile.classList.add('fa-moon');
            }
        }
    }

    // Toggle theme
    function toggleTheme() {
        console.log('Toggling theme...');
        const isDark = html.classList.contains('dark');
        const newTheme = isDark ? 'light' : 'dark';
        setTheme(newTheme);

        // Dispatch a custom event that other scripts can listen for
        window.dispatchEvent(new CustomEvent('themeChanged', {
            detail: { theme: newTheme }
        }));
    }

    // Make sure the toggle button is properly wired up
    function setupToggleButton() {
        const toggleBtn = document.getElementById('themeToggle');
        if (toggleBtn) {
            toggleBtn.removeEventListener('click', toggleTheme);
            toggleBtn.addEventListener('click', toggleTheme);
        }

        const toggleBtnMobile = document.getElementById('themeToggleMobile');
        if (toggleBtnMobile) {
            toggleBtnMobile.removeEventListener('click', toggleTheme);
            toggleBtnMobile.addEventListener('click', toggleTheme);
        }
    }

    // Run initialization when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            initTheme();
            setupToggleButton();
        });
    } else {
        initTheme();
        setupToggleButton();
    }

    // Also listen for system theme changes
    if (window.matchMedia) {
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
            // Only apply system preference if user hasn't set a preference
            const hasCookie = document.cookie.includes('.FuelMS.Theme=');
            const hasLocalStorage = localStorage.getItem('theme');

            if (!hasCookie && !hasLocalStorage) {
                if (e.matches) {
                    setTheme('dark');
                } else {
                    setTheme('light');
                }
            }
        });
    }

    // Expose to global scope
    window.themeManager = {
        init: initTheme,
        toggle: toggleTheme,
        setTheme: setTheme,
        getCurrentTheme: () => html.classList.contains('dark') ? 'dark' : 'light'
    };
})();

// Mobile Sidebar
(function () {
    const sidebar = document.getElementById('sidebar');
    const mobileMenuBtn = document.getElementById('mobileMenuBtn');
    const closeSidebarBtn = document.getElementById('closeSidebarBtn');

    if (mobileMenuBtn) {
        mobileMenuBtn.addEventListener('click', () => {
            sidebar?.classList.remove('-translate-x-full');
        });
    }

    if (closeSidebarBtn) {
        closeSidebarBtn.addEventListener('click', () => {
            sidebar?.classList.add('-translate-x-full');
        });
    }

    // Close sidebar when clicking outside on mobile
    document.addEventListener('click', (event) => {
        if (window.innerWidth < 768) {
            if (sidebar && !sidebar.contains(event.target) && !mobileMenuBtn?.contains(event.target)) {
                sidebar.classList.add('-translate-x-full');
            }
        }
    });

    // Handle window resize
    window.addEventListener('resize', () => {
        if (window.innerWidth >= 768) {
            sidebar?.classList.remove('-translate-x-full');
        } else {
            sidebar?.classList.add('-translate-x-full');
        }
    });
})();
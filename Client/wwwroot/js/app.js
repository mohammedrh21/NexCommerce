window.nexCommerce = {
    // Dark mode settings
    initializeTheme: function () {
        if (localStorage.getItem('theme') === 'dark' || 
            (!('theme' in localStorage) && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
    },
    setTheme: function (theme) {
        if (theme === 'dark') {
            document.documentElement.classList.add('dark');
            localStorage.setItem('theme', 'dark');
        } else {
            document.documentElement.classList.remove('dark');
            localStorage.setItem('theme', 'light');
        }
    },
    getTheme: function () {
        return document.documentElement.classList.contains('dark') ? 'dark' : 'light';
    },

    // Click outside handler for dropdowns/modals
    registerClickOutside: function (elementId, dotNetHelper, methodName) {
        window.addEventListener('click', function (e) {
            const el = document.getElementById(elementId);
            if (el && !el.contains(e.target)) {
                dotNetHelper.invokeMethodAsync(methodName);
            }
        });
    },

    // Scroll helpers
    scrollToTop: function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    },

    // Clipboard copy helper
    copyToClipboard: async function (text) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch (err) {
            return false;
        }
    }
};

// Auto-run theme initialization
window.nexCommerce.initializeTheme();

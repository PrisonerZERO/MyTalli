// Theme management — applies data-theme attribute to <html>
window.themeManager = {
    apply: function (mode) {
        localStorage.setItem('talli-theme-mode', mode);
        var resolved = mode;
        if (mode === 'system') {
            resolved = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        }
        document.documentElement.setAttribute('data-theme', resolved === 'dark' ? 'dark' : '');
        document.documentElement.setAttribute('data-theme-mode', mode);
        // Update theme-color meta tag for browser chrome
        var meta = document.querySelector('meta[name="theme-color"]');
        if (meta) {
            meta.setAttribute('content', resolved === 'dark' ? '#0f0f1a' : '#6c5ce7');
        }
    },
    getSystem: function () {
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }
};

// Auto-apply saved theme on load and after Blazor enhanced navigation
(function () {
    function reapply() {
        var saved = localStorage.getItem('talli-theme-mode');
        if (saved) {
            window.themeManager.apply(saved);
        }
    }
    reapply();
    if (typeof Blazor !== 'undefined') {
        Blazor.addEventListener('enhancedload', reapply);
    }
})();

// Listen for OS theme changes when in system mode
window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function () {
    var current = document.documentElement.getAttribute('data-theme-mode');
    if (current === 'system') {
        window.themeManager.apply('system');
    }
});

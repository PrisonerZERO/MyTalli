// Theme management — applies data-theme attribute to <html>.
// Cookie 'talli-theme' is the single source of truth and is only present for authenticated users
// (set by OAuth handlers + Settings on sign-in/save, deleted by CurrentUserMiddleware on every
// unauthenticated request). Routes / and /signin always render light because their CSS has no
// dark mode coverage.
window.themeManager = {
    apply: function (mode) {
        if (isAlwaysLightRoute()) { clearTheme(); return; }
        document.cookie = 'talli-theme=' + mode + '; path=/; max-age=2592000; SameSite=Lax';
        var resolved = mode;
        if (mode === 'system') {
            resolved = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        }
        document.documentElement.setAttribute('data-theme', resolved === 'dark' ? 'dark' : '');
        document.documentElement.setAttribute('data-theme-mode', mode);
        var meta = document.querySelector('meta[name="theme-color"]');
        if (meta) {
            meta.setAttribute('content', resolved === 'dark' ? '#0f0f1a' : '#6c5ce7');
        }
    },
    getSystem: function () {
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }
};

function isAlwaysLightRoute() {
    var path = window.location.pathname.toLowerCase();
    return path === '/' || path === '/signin';
}

function getCookie(name) {
    var match = document.cookie.match(new RegExp('(?:^|; )' + name + '=([^;]*)'));
    return match ? match[1] : null;
}

function clearTheme() {
    document.documentElement.removeAttribute('data-theme');
    document.documentElement.removeAttribute('data-theme-mode');
    var meta = document.querySelector('meta[name="theme-color"]');
    if (meta) {
        meta.setAttribute('content', '#6c5ce7');
    }
}

(function () {
    function reapply() {
        if (isAlwaysLightRoute()) { clearTheme(); return; }
        var saved = getCookie('talli-theme');
        if (saved) {
            window.themeManager.apply(saved);
        } else {
            clearTheme();
        }
    }
    reapply();
    if (typeof Blazor !== 'undefined') {
        Blazor.addEventListener('enhancedload', reapply);
    }
})();

// Listen for OS theme changes when in system mode.
window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function () {
    var current = document.documentElement.getAttribute('data-theme-mode');
    if (current === 'system') {
        window.themeManager.apply('system');
    }
});

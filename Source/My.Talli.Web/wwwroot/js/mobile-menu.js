// Mobile hamburger menu — toggles sidebar slide-in via CSS classes.
// Uses event delegation so it works regardless of Blazor render mode.
document.addEventListener('click', function (e) {
    var hamburger = e.target.closest('.mobile-hamburger');
    var backdrop = e.target.closest('.mobile-backdrop');
    var navLink = e.target.closest('.sidebar .nav-link');

    if (hamburger) {
        var sidebar = document.querySelector('.sidebar');
        var bg = document.querySelector('.mobile-backdrop');
        var isOpen = sidebar && sidebar.classList.contains('mobile-open');
        if (sidebar) sidebar.classList.toggle('mobile-open');
        if (bg) bg.classList.toggle('active');
        hamburger.setAttribute('aria-label', isOpen ? 'Open menu' : 'Close menu');
        hamburger.innerHTML = isOpen
            ? '<svg aria-hidden="true" fill="none" stroke="currentColor" stroke-linecap="round" stroke-width="2" viewBox="0 0 24 24"><line x1="3" x2="21" y1="6" y2="6"></line><line x1="3" x2="21" y1="12" y2="12"></line><line x1="3" x2="21" y1="18" y2="18"></line></svg>'
            : '<svg aria-hidden="true" fill="none" stroke="currentColor" stroke-linecap="round" stroke-width="2" viewBox="0 0 24 24"><line x1="18" x2="6" y1="6" y2="18"></line><line x1="6" x2="18" y1="6" y2="18"></line></svg>';
    }
    if (backdrop || navLink) {
        var sidebar = document.querySelector('.sidebar');
        var bg = document.querySelector('.mobile-backdrop');
        var btn = document.querySelector('.mobile-hamburger');
        if (sidebar) sidebar.classList.remove('mobile-open');
        if (bg) bg.classList.remove('active');
        if (btn) {
            btn.setAttribute('aria-label', 'Open menu');
            btn.innerHTML = '<svg aria-hidden="true" fill="none" stroke="currentColor" stroke-linecap="round" stroke-width="2" viewBox="0 0 24 24"><line x1="3" x2="21" y1="6" y2="6"></line><line x1="3" x2="21" y1="12" y2="12"></line><line x1="3" x2="21" y1="18" y2="18"></line></svg>';
        }
    }
});

// Top-of-viewport loading bar that pulses during Blazor enhanced navigation.
// Shows the moment a user clicks an internal link (no waiting on server),
// hides when Blazor's enhancedload event fires.
//
// Pattern: 3px purple gradient bar with an indeterminate shimmer animation.

(function () {
    var BAR_ID = 'nav-loader';
    var SHOW_CLASS = 'nav-loader-active';
    var SAFETY_TIMEOUT_MS = 15000; // hide regardless after this — guards against missed enhancedload events

    var bar = document.getElementById(BAR_ID);
    if (!bar) return;

    var safetyTimer = null;

    function showBar() {
        bar.classList.add(SHOW_CLASS);
        if (safetyTimer) clearTimeout(safetyTimer);
        safetyTimer = setTimeout(hideBar, SAFETY_TIMEOUT_MS);
    }

    function hideBar() {
        bar.classList.remove(SHOW_CLASS);
        if (safetyTimer) {
            clearTimeout(safetyTimer);
            safetyTimer = null;
        }
    }

    // Trigger on click of any internal anchor (Blazor's enhanced nav will pick it up).
    // We catch in the capture phase so we run before any preventDefault.
    document.addEventListener('click', function (e) {
        // Walk up from the click target to find an <a> ancestor
        var el = e.target;
        while (el && el !== document) {
            if (el.tagName === 'A') break;
            el = el.parentNode;
        }
        if (!el || el.tagName !== 'A') return;

        var href = el.getAttribute('href');
        if (!href) return;

        // Skip external links, downloads, mailto, tel, javascript, anchors-only, target=_blank, modified clicks
        if (href.startsWith('http://') || href.startsWith('https://')) {
            // Only treat as internal if same origin
            try {
                var url = new URL(href);
                if (url.origin !== window.location.origin) return;
            } catch (_) {
                return;
            }
        }
        if (href.startsWith('mailto:') || href.startsWith('tel:') || href.startsWith('javascript:') || href.startsWith('#')) return;
        if (el.hasAttribute('download')) return;
        if (el.target === '_blank') return;
        if (e.metaKey || e.ctrlKey || e.shiftKey || e.altKey || e.button !== 0) return;

        showBar();
    }, true);

    // Hide when Blazor enhanced navigation finishes loading the new page.
    // Blazor.addEventListener exists once blazor.web.js has loaded — wait for it.
    function bindBlazorEvents() {
        if (typeof Blazor === 'undefined' || !Blazor.addEventListener) {
            setTimeout(bindBlazorEvents, 50);
            return;
        }
        Blazor.addEventListener('enhancedload', hideBar);
    }
    bindBlazorEvents();

    // Also hide if the browser navigates back/forward (popstate triggers enhanced nav too).
    window.addEventListener('pageshow', hideBar);
})();

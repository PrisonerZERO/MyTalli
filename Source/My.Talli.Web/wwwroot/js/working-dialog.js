// Full-viewport "Working…" modal that shows during Blazor enhanced navigation.
//
// Triggers the moment a user clicks an internal link (no server roundtrip)
// and hides on Blazor's enhancedload event. Minimum visible duration of
// 2000ms prevents a sub-flash on fast loads.
//
// Defends against Blazor's enhanced-navigation DOM merge stripping the
// active class mid-navigation via a MutationObserver that re-applies it
// while the dialog is "supposed to be" visible.

(function () {
    var DIALOG_ID = 'working-dialog';
    var SHOW_CLASS = 'working-dialog-active';
    var MIN_VISIBLE_MS = 2000;
    var SAFETY_TIMEOUT_MS = 15000;

    var dialog = document.getElementById(DIALOG_ID);
    if (!dialog) return;

    var safetyTimer = null;
    var shownAt = 0;
    var pendingHide = null;
    var shouldBeVisible = false; // tracked separately from class — survives Blazor's DOM merge

    function showDialog() {
        shouldBeVisible = true;
        if (pendingHide) {
            clearTimeout(pendingHide);
            pendingHide = null;
        }
        applyVisibleState();
        shownAt = Date.now();

        if (safetyTimer) clearTimeout(safetyTimer);
        safetyTimer = setTimeout(forceHide, SAFETY_TIMEOUT_MS);
    }

    function applyVisibleState() {
        dialog.classList.add(SHOW_CLASS);
        dialog.setAttribute('aria-hidden', 'false');
    }

    function scheduleHide() {
        var elapsed = Date.now() - shownAt;
        var remaining = MIN_VISIBLE_MS - elapsed;

        // Re-apply visible state in case Blazor's DOM merge stripped it during navigation
        if (remaining > 0) {
            applyVisibleState();
            if (pendingHide) clearTimeout(pendingHide);
            pendingHide = setTimeout(forceHide, remaining);
        } else {
            forceHide();
        }
    }

    function forceHide() {
        shouldBeVisible = false;
        if (safetyTimer) {
            clearTimeout(safetyTimer);
            safetyTimer = null;
        }
        if (pendingHide) {
            clearTimeout(pendingHide);
            pendingHide = null;
        }
        dialog.classList.remove(SHOW_CLASS);
        dialog.setAttribute('aria-hidden', 'true');
    }

    // MutationObserver — if Blazor's enhanced-nav DOM merge strips the active class
    // while we're supposed to be visible, re-add it immediately. This is the primary
    // defense against the "dialog disappears too quickly" bug.
    var observer = new MutationObserver(function (mutations) {
        if (!shouldBeVisible) return;
        for (var i = 0; i < mutations.length; i++) {
            var m = mutations[i];
            if (m.attributeName === 'class' && !dialog.classList.contains(SHOW_CLASS)) {
                applyVisibleState();
            }
        }
    });
    observer.observe(dialog, { attributes: true, attributeFilter: ['class', 'aria-hidden'] });

    // Trigger on click of any internal anchor.
    document.addEventListener('click', function (e) {
        var el = e.target;
        while (el && el !== document) {
            if (el.tagName === 'A') break;
            el = el.parentNode;
        }
        if (!el || el.tagName !== 'A') return;

        var href = el.getAttribute('href');
        if (!href) return;

        if (href.startsWith('http://') || href.startsWith('https://')) {
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

        showDialog();
    }, true);

    function bindBlazorEvents() {
        if (typeof Blazor === 'undefined' || !Blazor.addEventListener) {
            setTimeout(bindBlazorEvents, 50);
            return;
        }
        Blazor.addEventListener('enhancedload', scheduleHide);
    }
    bindBlazorEvents();

    window.addEventListener('pageshow', forceHide);
})();

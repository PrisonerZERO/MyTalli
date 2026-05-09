(function () {
    function bind() {
        if (window.location.pathname.toLowerCase() !== '/signin') return;

        var buttons = document.querySelectorAll('.login-btn[data-provider]');
        if (!buttons.length) return;
        if (buttons[0].dataset.signinBound === '1') return;

        var hint = document.getElementById('login-hint');
        var spinner = document.getElementById('login-spinner');
        var spinnerText = document.getElementById('login-spinner-text');
        var clicked = false;

        buttons.forEach(function (btn) {
            btn.dataset.signinBound = '1';
            btn.addEventListener('click', function (e) {
                if (clicked) { e.preventDefault(); e.stopImmediatePropagation(); return; }
                clicked = true;
                e.preventDefault();
                e.stopImmediatePropagation();

                var provider = btn.getAttribute('data-provider');
                var href = btn.getAttribute('href');

                buttons.forEach(function (b) {
                    b.classList.add('is-disabled');
                    b.setAttribute('aria-disabled', 'true');
                    b.setAttribute('tabindex', '-1');
                });

                if (hint) hint.setAttribute('hidden', '');
                if (spinnerText) spinnerText.textContent = 'Logging in with ' + provider + '...';
                if (spinner) spinner.removeAttribute('hidden');

                requestAnimationFrame(function () {
                    requestAnimationFrame(function () { window.location.href = href; });
                });
            });
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', bind);
    } else {
        bind();
    }

    if (window.Blazor && typeof window.Blazor.addEventListener === 'function') {
        window.Blazor.addEventListener('enhancedload', bind);
    } else {
        document.addEventListener('DOMContentLoaded', function () {
            if (window.Blazor && typeof window.Blazor.addEventListener === 'function') {
                window.Blazor.addEventListener('enhancedload', bind);
            }
        });
    }
})();

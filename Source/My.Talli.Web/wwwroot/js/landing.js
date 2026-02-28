let scrollHandler = null;

export function initLanding() {
    const navEl = document.getElementById('landing-nav');
    if (!navEl) return;

    const lightSections = document.querySelectorAll('.how-section, .features-section, .bottom-cta');
    const darkSections = document.querySelectorAll('.pricing-section');

    scrollHandler = function () {
        let onLight = false;
        let onDark = false;
        lightSections.forEach(section => {
            const rect = section.getBoundingClientRect();
            if (rect.top < 64 && rect.bottom > 0) onLight = true;
        });
        darkSections.forEach(section => {
            const rect = section.getBoundingClientRect();
            if (rect.top < 64 && rect.bottom > 0) onDark = true;
        });
        navEl.classList.toggle('on-light', onLight);
        navEl.classList.toggle('on-dark', onDark);
    };

    window.addEventListener('scroll', scrollHandler);
    scrollHandler();
}

export function highlightWaitlist() {
    const input = document.querySelector('#waitlist input');
    if (!input) return;

    if (window.scrollY < 10) {
        input.classList.add('highlight');
        input.focus();
        requestAnimationFrame(() => {
            setTimeout(() => {
                input.classList.remove('highlight');
            }, 50);
        });
    } else {
        window.scrollTo({ top: 0, behavior: 'smooth' });
        const checkScroll = setInterval(() => {
            if (window.scrollY < 10) {
                clearInterval(checkScroll);
                input.classList.add('highlight');
                input.focus();
                requestAnimationFrame(() => {
                    setTimeout(() => {
                        input.classList.remove('highlight');
                    }, 50);
                });
            }
        }, 50);
    }
}

export function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

export function scrollToSection(sectionId) {
    const section = document.getElementById(sectionId);
    if (section) {
        section.scrollIntoView({ behavior: 'smooth' });
    }
}

export function dispose() {
    if (scrollHandler) {
        window.removeEventListener('scroll', scrollHandler);
        scrollHandler = null;
    }
}

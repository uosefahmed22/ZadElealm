document.addEventListener('DOMContentLoaded', () => {
    const toggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('layoutSidenav_nav');
    const content = document.getElementById('layoutSidenav_content');
    const mobileQuery = window.matchMedia('(max-width: 991.98px)');
    if (!toggle || !sidebar || !content) return;

    const syncAccessibilityState = () => {
        const isOpen = mobileQuery.matches
            ? document.body.classList.contains('sb-sidenav-toggled')
            : !document.body.classList.contains('sb-sidenav-toggled');
        toggle.setAttribute('aria-expanded', isOpen.toString());
    };

    toggle.addEventListener('click', event => {
        event.preventDefault();
        document.body.classList.toggle('sb-sidenav-toggled');
        syncAccessibilityState();
    });

    sidebar.querySelectorAll('a').forEach(link => {
        link.addEventListener('click', () => {
            if (mobileQuery.matches) {
                document.body.classList.remove('sb-sidenav-toggled');
                syncAccessibilityState();
            }
        });
    });

    content.addEventListener('click', () => {
        if (mobileQuery.matches && document.body.classList.contains('sb-sidenav-toggled')) {
            document.body.classList.remove('sb-sidenav-toggled');
            syncAccessibilityState();
        }
    });

    document.addEventListener('keydown', event => {
        if (event.key === 'Escape' && mobileQuery.matches) {
            document.body.classList.remove('sb-sidenav-toggled');
            syncAccessibilityState();
            toggle.focus();
        }
    });

    mobileQuery.addEventListener('change', () => {
        document.body.classList.remove('sb-sidenav-toggled');
        syncAccessibilityState();
    });
    syncAccessibilityState();
});

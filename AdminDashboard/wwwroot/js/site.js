// التأكد من تحميل الصفحة بالكامل
document.addEventListener("DOMContentLoaded", function () {
    const sidebarToggle = document.getElementById('sidebarToggle');
    const layoutSidenav = document.getElementById('layoutSidenav');
    const layoutSidenavNav = document.getElementById('layoutSidenav_nav');
    const layoutSidenavContent = document.getElementById('layoutSidenav_content');

    // حالة القائمة (مفتوحة/مغلقة)
    let isSidebarOpen = true;

    sidebarToggle.addEventListener('click', function () {
        isSidebarOpen = !isSidebarOpen;

        if (!isSidebarOpen) {
            // إخفاء القائمة
            layoutSidenavNav.style.transform = 'translateX(-225px)';
            layoutSidenavContent.style.marginLeft = '0';
            layoutSidenavContent.style.width = '100%';
        } else {
            // إظهار القائمة
            layoutSidenavNav.style.transform = 'translateX(0)';
            layoutSidenavContent.style.marginLeft = '225px';
        }
    });
});
document.addEventListener('DOMContentLoaded', function () {
    // Toggle sidebar functionality
    const sidebarToggle = document.getElementById('sidebarToggle');
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function (e) {
            e.preventDefault();
            document.body.classList.toggle('sb-sidenav-toggled');
        });
    }
});
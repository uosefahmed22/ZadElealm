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
// في نفس الملف site.js
document.addEventListener('DOMContentLoaded', function () {
    // Search functionality
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('keyup', function () {
            const searchText = this.value.toLowerCase();
            const table = document.getElementById('rolesTable');
            const rows = table.getElementsByTagName('tr');

            for (let i = 1; i < rows.length; i++) {
                const roleNameCell = rows[i].getElementsByTagName('td')[1];
                if (roleNameCell) {
                    const roleName = roleNameCell.textContent || roleNameCell.innerText;
                    if (roleName.toLowerCase().indexOf(searchText) > -1) {
                        rows[i].style.display = '';
                    } else {
                        rows[i].style.display = 'none';
                    }
                }
            }
        });
    }
});

// Delete role function
function deleteRole(roleId) {
    if (confirm('Are you sure you want to delete this role?')) {
        window.location.href = `/Role/Delete/${roleId}`;
    }
}
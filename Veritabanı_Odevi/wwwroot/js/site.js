// wwwroot/js/site.js - Basit versiyon
document.addEventListener('DOMContentLoaded', function () {
    console.log('Site yüklendi - Dark Theme aktif');

    // Smooth scroll
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const targetId = this.getAttribute('href');
            if (targetId === '#') return;

            const targetElement = document.querySelector(targetId);
            if (targetElement) {
                window.scrollTo({
                    top: targetElement.offsetTop - 80,
                    behavior: 'smooth'
                });
            }
        });
    });

    // Dropdown menüler için
    document.querySelectorAll('.dropdown-submenu > a').forEach(element => {
        element.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            // Diğer açık alt menüleri kapat
            this.parentElement.parentElement.querySelectorAll('.dropdown-menu.show').forEach(submenu => {
                if (submenu !== this.nextElementSibling) {
                    submenu.classList.remove('show');
                }
            });

            // Tıklanan alt menüyü aç/kapat
            this.nextElementSibling.classList.toggle('show');
        });
    });
});
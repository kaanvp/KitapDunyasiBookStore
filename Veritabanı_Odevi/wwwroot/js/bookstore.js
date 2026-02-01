// wwwroot/js/bookstore.js - DÜZENLENMİŞ VERSİYON
// Mevcut projenizdeki sınıf isimlerine uygun

// Sepet işlemleri için global fonksiyonlar
const BookStore = {
    // Sepete ekle
    addToCart: function (bookId, quantity = 1) {
        // CSRF token al
        const token = this.getAntiForgeryToken();

        fetch('/Cart/AddToCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ bookId: bookId, quantity: quantity })
        })
            .then(response => response.json())
            .then(data => {
                this.updateCartUI(data);
                this.showToast('Kitap sepete eklendi!', 'success');
            })
            .catch(error => {
                console.error('Error:', error);
                this.showToast('Sepete eklenemedi', 'error');
            });
    },

    // Sepetten kaldır
    removeFromCart: function (cartItemId) {
        if (confirm('Bu ürünü sepetten kaldırmak istediğinize emin misiniz?')) {
            fetch(`/Cart/RemoveFromCart/${cartItemId}`, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': this.getAntiForgeryToken()
                }
            })
                .then(response => response.json())
                .then(data => {
                    this.updateCartUI(data);
                    this.showToast('Ürün sepetten kaldırıldı', 'info');
                })
                .catch(error => {
                    console.error('Error:', error);
                    this.showToast('Silme işlemi başarısız', 'error');
                });
        }
    },

    // Sepet UI güncelle
    updateCartUI: function (cartData) {
        // Sepet sayacını güncelle
        const cartCount = document.querySelector('.cart-count');
        if (cartCount) {
            cartCount.textContent = cartData.itemCount || 0;
        }

        // Sepet toplamını güncelle
        const cartTotal = document.querySelector('.cart-total');
        if (cartTotal) {
            cartTotal.textContent = cartData.total ? cartData.total.toFixed(2) + ' ₺' : '0.00 ₺';
        }
    },

    // Toast bildirimi göster
    showToast: function (message, type = 'success') {
        // Bootstrap toast kullanıyorsanız
        const toastEl = document.querySelector('.toast');
        if (toastEl) {
            const toastBody = toastEl.querySelector('.toast-body');
            if (toastBody) {
                toastBody.textContent = message;
            }

            // Renk sınıfını ayarla
            toastEl.className = 'toast align-items-center text-white border-0';
            if (type === 'success') {
                toastEl.classList.add('bg-success');
            } else if (type === 'error') {
                toastEl.classList.add('bg-danger');
            } else {
                toastEl.classList.add('bg-info');
            }

            const toast = new bootstrap.Toast(toastEl);
            toast.show();
        } else {
            // Fallback: alert
            alert(message);
        }
    },

    // CSRF token al
    getAntiForgeryToken: function () {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    },

    // Kitap slider'ını başlat (isteğe bağlı)
    initBookSlider: function () {
        const sliderContainer = document.querySelector('.book-slider-section');
        if (!sliderContainer) return;

        const track = sliderContainer.querySelector('.book-slider-track');
        const cards = sliderContainer.querySelectorAll('.book-slider-card');
        const prevBtn = sliderContainer.querySelector('.slider-nav-btn.prev');
        const nextBtn = sliderContainer.querySelector('.slider-nav-btn.next');

        if (!track || cards.length === 0) return;

        let currentIndex = 0;
        const cardWidth = cards[0].offsetWidth;
        const gap = 20;

        function updateSlider(index) {
            cards.forEach((card, i) => {
                card.classList.toggle('active', i === index);
            });

            const offset = -(index * (cardWidth + gap));
            const centerOffset = (track.parentElement.offsetWidth - cardWidth) / 2;
            track.style.transform = `translateX(${offset + centerOffset}px)`;
            currentIndex = index;
        }

        if (prevBtn) {
            prevBtn.addEventListener('click', () => {
                currentIndex = (currentIndex - 1 + cards.length) % cards.length;
                updateSlider(currentIndex);
            });
        }

        if (nextBtn) {
            nextBtn.addEventListener('click', () => {
                currentIndex = (currentIndex + 1) % cards.length;
                updateSlider(currentIndex);
            });
        }

        // Otomatik geçiş
        setInterval(() => {
            currentIndex = (currentIndex + 1) % cards.length;
            updateSlider(currentIndex);
        }, 5000);

        // İlk görünüm
        updateSlider(0);
    },

    // Tüm event listener'ları başlat
    init: function () {
        console.log('BookStore initialized');

        // Sepete ekle butonlarına event bağla
        document.querySelectorAll('.btn-add-to-cart').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const bookId = btn.dataset.bookId;
                if (bookId) {
                    this.addToCart(bookId, 1);
                }
            });
        });

        // Sepetten kaldır butonları
        document.querySelectorAll('.btn-remove-from-cart').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                const cartItemId = btn.dataset.cartItemId;
                if (cartItemId) {
                    this.removeFromCart(cartItemId);
                }
            });
        });

        // Slider'ı başlat (varsa)
        this.initBookSlider();
    }
};

// Sayfa yüklendiğinde başlat
document.addEventListener('DOMContentLoaded', function () {
    BookStore.init();

    // Form submit'lerini yönet
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>İşleniyor...';
            }
        });
    });
});
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Veritabani_Odevi.Data;
using Veritabani_Odevi.Filters;
using Veritabani_Odevi.Models;
using Veritabani_Odevi.Models.ViewModels;

namespace Veritabani_Odevi.Controllers
{
    [GirisGerekli]
    public class SepetController : Controller
    {
        private readonly SepetRepository _sepetRepository;
        private readonly KitapRepository _kitapRepository;
        public SepetController(SepetRepository sepetRepository, KitapRepository kitapRepository)
        {
            _sepetRepository = sepetRepository;
            _kitapRepository = kitapRepository;
        }
        // 🛒 Sepet Sayfası
        public IActionResult Index()
        {
            int kullaniciID = HttpContext.Session.GetInt32("KullaniciID") ?? 0;

            if (kullaniciID == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            var sepet = _sepetRepository.SepetiGetir(kullaniciID);
            var viewModel = sepet.Select(x => new SepetElemaniViewModel
            {
                KitapID = x.KitapID,
                Adet = x.Adet,
                Baslik = x.kitap.Baslik,
                OrijinalFiyat = x.OrijinalFiyat,
                IndirimliFiyat = x.IndirimliFiyat,
                IndirimOrani = x.IndirimOrani,
                KapakResmiUrl = x.kitap.KapakResmiUrl
            }).ToList();

            return View(viewModel);
        }


        // ➕ Sepete Ekle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SepeteEkle(int kitapID, int adet = 1)
        {
            int kullaniciID = HttpContext.Session.GetInt32("KullaniciID") ?? 0;

            if (kullaniciID == 0)
            {
                TempData["Error"] = "Sepete eklemek için giriş yapmalısınız.";
                return RedirectToAction("GirisKayit", "Kullanici");
            }

            var kitap = _kitapRepository.KitapGetir(kitapID);
            if (kitap == null)
            {
                TempData["Error"] = "Kitap bulunamadı.";
                return RedirectToAction("Index", "Kitap");
            }

            var sepetElemani = new SepetElemani
            {
                KullaniciID = kullaniciID,
                KitapID = kitapID,
                Adet = adet
            };

            _sepetRepository.SepeteEkle(sepetElemani);

            TempData["Success"] = "Kitap sepete eklendi.";
            string referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }
            return RedirectToAction("KitapListele", "Kitap");
        }

        // ❌ Sepetten Sil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SepettenSil(int kitapID)
        {
            int kullaniciID = HttpContext.Session.GetInt32("KullaniciID") ?? 0;

            if (kullaniciID == 0)
            {
                return RedirectToAction("Giris", "Hesap");
            }

            _sepetRepository.SepettenSil(kullaniciID, kitapID);

            TempData["Success"] = "Ürün sepetten kaldırıldı.";
            return RedirectToAction("Index");
        }

        // 🧹 Sepeti Temizle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SepetiTemizle()
        {
            int kullaniciID = HttpContext.Session.GetInt32("KullaniciID") ?? 0;

            if (kullaniciID == 0)
            {
                return RedirectToAction("Giris", "Hesap");
            }

            _sepetRepository.SepetiTemizle(kullaniciID);

            TempData["Success"] = "Sepet temizlendi.";
            return RedirectToAction("Index");
        }

        // ➕ Adet Artır
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdetArtir(int kitapID)
        {
            int kullaniciID = HttpContext.Session.GetInt32("KullaniciID") ?? 0;

            if (kullaniciID == 0)
            {
                return RedirectToAction("Giris", "Hesap");
            }

            var sepet = _sepetRepository.SepetiGetir(kullaniciID);
            var item = sepet.FirstOrDefault(x => x.KitapID == kitapID);
            if (item != null)
            {
                _sepetRepository.AdetGuncelle(kullaniciID, kitapID, item.Adet + 1);
            }

            return RedirectToAction("Index");
        }

        // ➖ Adet Azalt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdetAzalt(int kitapID)
        {
            int kullaniciID = HttpContext.Session.GetInt32("KullaniciID") ?? 0;

            if (kullaniciID == 0)
            {
                return RedirectToAction("Giris", "Hesap");
            }

            var sepet = _sepetRepository.SepetiGetir(kullaniciID);
            var item = sepet.FirstOrDefault(x => x.KitapID == kitapID);
            if (item != null)
            {
                _sepetRepository.AdetGuncelle(kullaniciID, kitapID, item.Adet - 1);
            }

            return RedirectToAction("Index");
        }

    }
}


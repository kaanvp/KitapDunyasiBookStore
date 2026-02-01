using Microsoft.AspNetCore.Mvc;
using Veritabani_Odevi.Data;
using Veritabani_Odevi.Filters;
using Veritabani_Odevi.Models;

namespace Veritabani_Odevi.Controllers
{
    public class YorumController : Controller
    {
        private readonly YorumRepository _yorumRepository;
        private readonly RaporRepository _raporRepository;

        public YorumController(
            YorumRepository yorumRepository,
            RaporRepository raporRepository)
        {
            _yorumRepository = yorumRepository;
            _raporRepository = raporRepository;
        }

        // =========================
        // KİTABA AİT YORUMLAR
        // =========================
        public IActionResult KitapYorumlari(int kitapID)
        {
            var yorumlar = _yorumRepository.KitapYorumlariniGetir(kitapID);
            ViewBag.KitapID = kitapID;
            return View(yorumlar);
        }

        // =========================
        // YORUM EKLE (GET)
        // =========================
        [HttpGet]
        [GirisGerekli]
        public IActionResult Ekle(int kitapID)
        {
            return View(new Yorum { KitapID = kitapID });
        }

        // =========================
        // YORUM EKLE (POST)
        // =========================
        [HttpPost]
        [GirisGerekli]
        public IActionResult Ekle(Yorum yorum)
        {
            ModelState.Remove("KullaniciAdSoyad");
            if (!ModelState.IsValid)
                return RedirectToAction("Detay", "Kitap", new { id = yorum.KitapID });

            try
            {
                _yorumRepository.YorumEkle(yorum);
                TempData["YorumMesaj"] = "Yorumunuz eklendi ✔";
            }
            catch (Exception ex)
            {
                // Aynı kitaba ikinci kez yorum yapıldığında
                TempData["YorumHata"] = ex.Message;
            }

            return RedirectToAction("Detay", "Kitap", new { id = yorum.KitapID });
        }

        // =========================
        // YORUM GÜNCELLE (GET)
        // =========================
        [HttpGet("Yorum/Guncelle/{yorumID}")]
        [GirisGerekli]
        public IActionResult Guncelle(int yorumID)
        {
            var yorum = _yorumRepository.GetById(yorumID);
            return View(yorum);
        }

        // =========================
        // YORUM GÜNCELLE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [GirisGerekli]
        public IActionResult Guncelle(Yorum yorum)
        {
            ModelState.Remove("KullaniciAdSoyad");
            if (!ModelState.IsValid)
                return View(yorum);

            bool sonuc = _yorumRepository.YorumGuncelle(yorum);

            if (sonuc)
            {
                TempData["Mesaj"] = "Yorum güncellendi.";
                return RedirectToAction("KitapListele","Kitap");
            }

            ModelState.AddModelError("", "Yorum güncellenemedi.");
            return View(yorum);
        }

        // =========================
        // YORUM SİL
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [GirisGerekli]
        public IActionResult Sil(int yorumID, int kullaniciID, int kitapID)
        {
            bool sonuc = _yorumRepository.YorumSil(yorumID, kullaniciID);

            TempData["Mesaj"] = sonuc
                ? "Yorum silindi."
                : "Yorum silinemedi.";

            return RedirectToAction("Listeleme", new { kullaniciID });
        }


        // =========================
        // ADMIN - YORUM ONAYLA
        // =========================
        [HttpGet("Yorum/Onayla/{yorumID}")]
        [AdminYetki]
        public IActionResult Onayla(int yorumID)
        {
            _yorumRepository.YorumOnayla(yorumID);
            TempData["Mesaj"] = "Yorum onaylandı.";
            return RedirectToAction("KitapListele", "Kitap");
        }

        // =========================
        // RAPOR - KATEGORİ YORUM
        // =========================
        [AdminYetki]
        public IActionResult KategoriYorumRaporu()
        {
            var rapor = _raporRepository.KategoriYorumRaporu();
            return View(rapor);
        }

        // =========================
        // KULLANICI YORUMLARI LİSTESİ
        // =========================
        public IActionResult Listeleme(int kullaniciID)
        {
            var yorumlar = _yorumRepository.KullaniciYorumlariniGetir(kullaniciID);
            ViewBag.KullaniciID = kullaniciID;
            return View(yorumlar);
        }
    }
}

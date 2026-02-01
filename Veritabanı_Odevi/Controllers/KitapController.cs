using Microsoft.AspNetCore.Mvc;
using Veritabani_Odevi.Data;
using Veritabani_Odevi.Filters;
using Veritabani_Odevi.Models;
using Veritabani_Odevi.Models.ViewModels;

namespace Veritabani_Odevi.Controllers
{
   
    public class KitapController : Controller
    {
        private readonly KitapRepository _kitapRepo;
        private readonly KategoriRepository _kategoriRepository;
        private readonly YorumRepository _yorumRepository;
        public KitapController(KitapRepository kitapRepository, KategoriRepository kategoriRepository, YorumRepository yorumRepository)
        {
            _kitapRepo = kitapRepository;
            _kategoriRepository = kategoriRepository;
            _yorumRepository = yorumRepository;
        }
        // işe yaramazsa sil
        [HttpGet]
        public IActionResult AktifIndirimdekiKitaplar()
        {
            var model = _kitapRepo.AktifIndirimdekiKitaplar();
            return View(model);
        }
        public IActionResult Ara(string aramaKelimesi, List<int> kategoriler,
                         decimal? minFiyat, decimal? maxFiyat,
                         string durum, string siralama = "YenidenEskiye")
        {
            var kitaplar = _kitapRepo.GelismisAramaIndirimli(
                aramaKelimesi, kategoriler, minFiyat, maxFiyat, durum, siralama);

            ViewBag.Kategoriler = _kategoriRepository.TumKategorileriGetir();
            ViewBag.AramaKelimesi = aramaKelimesi;
            ViewBag.MinFiyat = minFiyat;
            ViewBag.MaxFiyat = maxFiyat;
            return View(kitaplar);
        }
        // --- Yeni Eklendi: Ekle ---
        [HttpGet]
        [AdminYetki]
        public IActionResult Islemler()
        {
            // Kitap işlemleri sayfası
            return View();
        }

        [HttpGet]
        public IActionResult KitapListele()
        {
            // Tüm kitapları indirim bilgisiyle birlikte getir
            var kitaplar = _kitapRepo.TumKitaplariIndirimliGetir();
            return View(kitaplar);
        }
        [HttpGet]
        [AdminYetki]
        public IActionResult KitapEkle()
        {
            ViewBag.Kategoriler = _kategoriRepository.TumKategorileriGetir();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminYetki]
        public IActionResult KitapEkle(KitapEkleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Kategoriler = _kategoriRepository.TumKategorileriGetir();
                return View(model); // 🔥 ViewModel geri dönüyor
            }

            try
            {
                // 🔹 1️⃣ ViewModel → Entity dönüşümü
                var yeniKitap = new Kitap
                {
                    Baslik = model.Baslik,
                    Yazar = model.Yazar,
                    YayinEvi = model.YayinEvi,
                    ISBN = model.ISBN,
                    Fiyat = model.Fiyat,
                    StokAdedi = model.StokAdedi,
                    Durum = model.Durum,
                    Aciklama = model.Aciklama,
                    SaticiKullaniciID =
                        HttpContext.Session.GetInt32("KullaniciID") ?? 1
                };

                // 🔹 2️⃣ Kapak resmi işlemi
                if (model.KapakDosyasi != null && model.KapakDosyasi.Length > 0)
                {
                    string uzanti = Path.GetExtension(model.KapakDosyasi.FileName);
                    string dosyaAdi = Guid.NewGuid() + uzanti;
                    string klasor = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/books"
                    );

                    if (!Directory.Exists(klasor))
                        Directory.CreateDirectory(klasor);

                    string tamYol = Path.Combine(klasor, dosyaAdi);

                    using var stream = new FileStream(tamYol, FileMode.Create);
                    model.KapakDosyasi.CopyTo(stream);

                    yeniKitap.KapakResmiUrl = "/books/" + dosyaAdi;
                }

                // 🔹 3️⃣ Repository çağrısı
                int kitapID = _kitapRepo.KitapEkle(
                    yeniKitap,
                    model.KategoriIDler
                );

                if (kitapID > 0)
                {
                    TempData["Basarili"] = "📚 Kitap başarıyla eklendi.";
                    return RedirectToAction("KitapEkle");
                }

                ModelState.AddModelError("", "Kitap eklenirken hata oluştu.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            ViewBag.Kategoriler = _kategoriRepository.TumKategorileriGetir();
            return View(model);
        }


        [HttpGet]
        [AdminYetki]
        public IActionResult KitapDuzenle(int? id = null)
        {
            var tümuKitaplar = _kitapRepo.TumKitaplariGetir();
            Kitap seçiliKitap = null;

            if (id.HasValue)
            {
                seçiliKitap = _kitapRepo.KitapGetir(id.Value);
            }

            ViewBag.SeçiliKitap = seçiliKitap;
            ViewBag.Kategoriler = _kategoriRepository.TumKategorileriGetir();
            return View(tümuKitaplar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminYetki]
        public IActionResult KitapDuzenle(Kitap kitap, IFormFile? KapakDosyasi)
        {
            var mevcut = _kitapRepo.KitapGetir(kitap.KitapID);

            if (mevcut == null)
            {
                ModelState.AddModelError("", "Kitap bulunamadı.");
                var kitaplar = _kitapRepo.TumKitaplariGetir();
                ViewBag.Kategoriler = _kategoriRepository.TumKategorileriGetir();
                return View(kitaplar);
            }

            if (!ModelState.IsValid)
            {
                var kitaplar = _kitapRepo.TumKitaplariGetir();
                ViewBag.SeçiliKitap = mevcut;
                ViewBag.Kategoriler = _kategoriRepository.TumKategorileriGetir();
                return View(kitaplar);
            }

            kitap.SaticiKullaniciID = mevcut.SaticiKullaniciID;

            // Kapak resmi işlemi
            if (KapakDosyasi != null && KapakDosyasi.Length > 0)
            {
                string uzanti = Path.GetExtension(KapakDosyasi.FileName);
                string dosyaAdi = Guid.NewGuid() + uzanti;
                string klasor = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/books");

                if (!Directory.Exists(klasor))
                    Directory.CreateDirectory(klasor);

                string tamYol = Path.Combine(klasor, dosyaAdi);

                using var stream = new FileStream(tamYol, FileMode.Create);
                KapakDosyasi.CopyTo(stream);

                kitap.KapakResmiUrl = "/books/" + dosyaAdi;
            }
            else if (string.IsNullOrEmpty(kitap.KapakResmiUrl))
            {
                kitap.KapakResmiUrl = mevcut.KapakResmiUrl;
            }

            bool sonuc = _kitapRepo.KitapGuncelle(kitap);

            if (sonuc)
            {
                TempData["Basarili"] = "✅ Kitap başarıyla güncellendi.";
                return RedirectToAction("KitapDuzenle", new { id = kitap.KitapID });
            }

            ModelState.AddModelError("", "Güncelleme başarısız.");
            var kitaplarList = _kitapRepo.TumKitaplariGetir();
            ViewBag.SeçiliKitap = mevcut;
            ViewBag.Kategoriler = _kategoriRepository.TumKategorileriGetir();
            return View(kitaplarList);
        }


        // --- Yeni Eklendi: Sil (Soft Delete olarak uygulanmış) ---
        [HttpPost] // Genellikle silme işlemleri GET yerine POST ile yapılır
        [ValidateAntiForgeryToken]
        [AdminYetki]
        public IActionResult KitapSil(int id)
        {
            int saticiId = HttpContext.Session.GetInt32("KullaniciID") ?? 1;

            bool silindi = _kitapRepo.KitapSil(id, saticiId);

            if (silindi)
            {
                TempData["Mesaj"] = "Kitap başarıyla silindi (Stok durumu güncellendi).";
            }
            else
            {
                TempData["HataMesaji"] = "Kitap silinemedi. Belki de bu kitabı silmeye yetkiniz yoktur.";
            }

            return RedirectToAction("KitapDuzenle");
        }
        [HttpGet]
        public IActionResult Kitaplar()
        {
            var kitaplar = _kitapRepo.TumKitaplariGetir();
            return View(kitaplar);
        }
        [HttpGet]
        public IActionResult Detay(int id)
        {
            var kitap = _kitapRepo.KitapGetir(id);
            ViewBag.Yorumlar = _yorumRepository.KitapYorumlariniGetir(id);

            ViewBag.KullaniciID = HttpContext.Session.GetInt32("KullaniciID");

            return View(kitap);
        }




    }
}

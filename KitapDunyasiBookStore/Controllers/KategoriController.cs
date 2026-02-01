using Microsoft.AspNetCore.Mvc;
using Veritabani_Odevi.Data;
using Veritabani_Odevi.Filters;
using Veritabani_Odevi.Models;

namespace Veritabani_Odevi.Controllers
{
    public class KategoriController : Controller
    {
        private readonly KategoriRepository _kategoriRepository;
        public KategoriController(KategoriRepository kategoriRepository)
        {
            _kategoriRepository = kategoriRepository;
        }
        public  IActionResult KategoriListele()
        {
            var kategoriler =  _kategoriRepository.TumKategorileriGetir();
            if (kategoriler == null)
            {
                return NotFound(); // Kategori bulunamazsa 404 hatası döner.
            }
            return View(kategoriler);
        }
        [HttpGet]
        [AdminYetki]
        public IActionResult KategoriEkle()
        {
            // Üst kategori seçimi için
            ViewBag.Kategoriler = _kategoriRepository.TumKategorileriGetir();
            return View();
        }

        // =======================
        // EKLE (POST)
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminYetki]
        public IActionResult KategoriEkle(Kategori kategori)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Kategoriler = _kategoriRepository.TumKategorileriGetir();
                return View(kategori);
            }

            _kategoriRepository.KategoriEkle(kategori);
            return RedirectToAction("KategoriEkle");
        }

        // =======================
        // GÜNCELLE (GET)
        // =======================
        [HttpGet]
        [AdminYetki]
        public IActionResult KategoriGuncelle(int id)
        {
            var kategori = _kategoriRepository.KategoriGetir(id);
            if (kategori == null)
                return NotFound();

            ViewBag.Kategoriler = _kategoriRepository.TumKategorileriGetir();
            return View(kategori);
        }

        // =======================
        // GÜNCELLE (POST)
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminYetki]
        public IActionResult KategoriGuncelle(Kategori kategori)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Kategoriler = _kategoriRepository.TumKategorileriGetir();
                return View(kategori);
            }

            _kategoriRepository.KategoriGuncelle(kategori);
            return RedirectToAction("KategoriListele");
        }

        // =======================
        // SİL
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminYetki]
        public IActionResult KategoriSil(int id)
        {
            _kategoriRepository.KategoriSil(id);
            return RedirectToAction("KategoriListele");
        }

    }
}

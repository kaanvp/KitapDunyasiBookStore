using Microsoft.AspNetCore.Mvc;
using Veritabani_Odevi.Data;
using Veritabani_Odevi.Filters;
using Veritabani_Odevi.Models;

namespace Veritabani_Odevi.Controllers
{
    public class IndirimController : Controller
    {
        private readonly KitapRepository _kitapRepository;
        private readonly IndirimRepository _indirimRepository;

        public IndirimController(KitapRepository kitapRepository, IndirimRepository indirimRepository)
        {
            _kitapRepository = kitapRepository;
            _indirimRepository = indirimRepository;
        }

        // /Indirim/Aktif - Aktif indirimli kitaplar
        public IActionResult Aktif()
        {
            var liste = _kitapRepository.AktifIndirimdekiKitaplar();
            return View(liste);
        }

        // /Indirim/Islemler - İndirim işlemleri sayfası
        [AdminYetki]
        public IActionResult Islemler()
        {
            return View();
        }

        // /Indirim/KitabaIndirimUygula - Kitaba İndirim Uygula sayfası (GET)
        [AdminYetki]
        public IActionResult KitabaIndirimUygula()
        {
            ViewBag.Indirimler = _indirimRepository.AktifIndirimleriGetir();
            ViewBag.Kitaplar = _kitapRepository.TumKitaplariGetir();
            return View();
        }

        // /Indirim/Index - Tüm indirimler (Yönetim)
        [AdminYetki]
        public IActionResult Index()
        {
            var indirimliste = _indirimRepository.TumIndirimleriGetir();
            return View(indirimliste);
        }

        // /Indirim/Ekle (GET)
        [AdminYetki]
        public IActionResult Ekle()
        {
            return View();
        }

        // /Indirim/Ekle (POST)
        [HttpPost]
        [AdminYetki]
        public IActionResult Ekle(Indirim indirim)
        {
            if (ModelState.IsValid)
            {
                int indirimID = _indirimRepository.IndirimEkle(indirim);
                // Yeni indirim eklendikten sonra kitaba uygulama formunu göster
                ViewBag.IndirimID = indirimID;
                ViewBag.Kitaplar = _kitapRepository.TumKitaplariGetir();
                return View(indirim);
            }
            return View(indirim);
        }

        // /Indirim/Düzenle (GET)
        [AdminYetki]
        public IActionResult Duzenle(int id)
        {
            var indirim = _indirimRepository.IndirimGetir(id);
            if (indirim == null)
                return NotFound();
            // Kitap listesini ViewBag'e ekle
            ViewBag.Kitaplar = _kitapRepository.TumKitaplariGetir();
            return View(indirim);
        }

        // /Indirim/Düzenle (POST)
        [HttpPost]
        [AdminYetki]
        public IActionResult Duzenle(Indirim indirim)
        {
            if (ModelState.IsValid)
            {
                _indirimRepository.IndirimGuncelle(indirim);
                return RedirectToAction("Index");
            }
            return View(indirim);
        }

        // /Indirim/Sil
        [HttpPost]
        [AdminYetki]
        public IActionResult Sil(int id)
        {
            _indirimRepository.IndirimSil(id);
            return RedirectToAction("Index");
        }

        // /Indirim/KitabıIndirimeEkle - Bir kitaba indirim uygula
        [HttpPost]
        [AdminYetki]
        public IActionResult KitabıIndirimeEkle(int kitapID, int indirimID)
        {
            _indirimRepository.IndirimUygulaKitaba(indirimID, kitapID);
            return RedirectToAction("Detay", "Kitap", new { id = kitapID });
        }
    }
}


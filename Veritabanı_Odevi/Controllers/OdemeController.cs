using Microsoft.AspNetCore.Mvc;
using Veritabani_Odevi.Data;
using Veritabani_Odevi.Models;

namespace Veritabani_Odevi.Controllers
{
    public class OdemeController : Controller
    {
        private readonly OdemeRepository _odemeRepository;
        private readonly SiparisRepository _siparisRepository;

        public OdemeController(OdemeRepository odemeRepository,
                                SiparisRepository siparisRepository)
        {
            _odemeRepository = odemeRepository;
            _siparisRepository = siparisRepository;
        }

        public IActionResult Odeme(int siparisID)
        {
            ViewBag.SiparisID = siparisID;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OdemeYap(int siparisID, string odemeTuru)
        {
            // 1️⃣ Sipariş toplamını çek
            decimal toplamTutar = _siparisRepository.SiparisToplamTutarGetir(siparisID);

            // 2️⃣ Ödeme oluştur
            var odeme = new Odeme
            {
                SiparisID = siparisID,
                OdemeTuru = odemeTuru,
                Tutar = toplamTutar,
                BankaOnayKodu = Guid.NewGuid().ToString().Substring(0, 10),
                Durum = "Basarili"
            };

            // 3️⃣ Ödeme kaydet
            _odemeRepository.OdemeEkle(odeme);

            return RedirectToAction("Index", "Siparis");
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using Veritabani_Odevi.Data;
using Veritabani_Odevi.Filters;
using Veritabani_Odevi.Models.ViewModels;

namespace Veritabani_Odevi.Controllers
{
    [GirisGerekli]
    public class SiparisController : Controller
    {
        private readonly SiparisRepository _siparisRepository;
        private readonly SepetRepository _sepetRepository;

        public SiparisController(SiparisRepository siparisRepository, SepetRepository sepetRepository)
        {
            _siparisRepository = siparisRepository;
            _sepetRepository = sepetRepository;
        }

        // Kullanıcının tüm siparişleri (liste)
        public IActionResult Index()
        {
            int kullaniciID = Convert.ToInt32(HttpContext.Session.GetInt32("KullaniciID"));
            var siparisler = _siparisRepository.TariheGoreSiparisGetir(kullaniciID);
            return View(siparisler);
        }

        // Sipariş detayları
        public IActionResult Detay(int siparisID)
        {
            var model = _siparisRepository.SiparisDetaySayfaGetir(siparisID);
            return View(model);
        }

        // Sepetten sonra yönlendirilen sayfa (Onay)
        public IActionResult Onay()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Onay(string teslimatAdresi)
        {
            int kullaniciID = Convert.ToInt32(
                HttpContext.Session.GetInt32("KullaniciID")
            );

            var sepet = _sepetRepository.SepetiGetir(kullaniciID);

            if (!sepet.Any())
                return RedirectToAction("Index", "Sepet");

            try
            {
                int siparisID = _siparisRepository.SiparisOlustur(
                    kullaniciID,
                    teslimatAdresi,
                    sepet
                );

                // Sipariş başarılı olursa sepeti temizle
                _sepetRepository.SepetiTemizle(kullaniciID);

                return RedirectToAction("Odeme", "Odeme", new { siparisID });
            }
            catch (Exception ex)
            {
                // Stok yetersizliği veya diğer hatalar
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Sepet");
            }
        }
    }
}

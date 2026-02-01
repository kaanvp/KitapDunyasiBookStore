using Microsoft.AspNetCore.Mvc;
using Veritabani_Odevi.Data;
using Veritabani_Odevi.Filters;

namespace Veritabani_Odevi.Controllers
{
    [AdminYetki]
    public class RaporController : Controller
    {
        private readonly RaporRepository _raporRepo;
        private readonly KullaniciRaporRepository _kullaniciRaporRepo;

        public RaporController(RaporRepository raporRepo, KullaniciRaporRepository kullaniciRaporRepo)
        {
            _raporRepo = raporRepo;
            _kullaniciRaporRepo = kullaniciRaporRepo;
        }

        // Raporlar Ana Sayfa
        public IActionResult Index()
        {
            return View();
        }

        // Kategori İstatistik action'ı
        public IActionResult KategoriIstatistik()
        {
           var model = _raporRepo.KategoriBazliSatisRaporu();
           return View("KategoriIstatistik", model);
        }

        public IActionResult KategoriBazliSatisRaporu()
        {
            var model = _raporRepo.KategoriBazliSatisRaporu();
            return View(model);
        }

        public IActionResult KategoriYorumRaporu()
        {
            var model = _raporRepo.KategoriYorumRaporu();
            ViewBag.GenelFarkliYorumcu = _raporRepo.GenelFarkliYorumcuSayisi();
            return View(model);
        }

        public IActionResult EnAktifKullanicilar(int minYorum = 2)
        {
            var model = _kullaniciRaporRepo.EnAktifKullanicilar(minYorum);
            return View(model);
        }
    }
}

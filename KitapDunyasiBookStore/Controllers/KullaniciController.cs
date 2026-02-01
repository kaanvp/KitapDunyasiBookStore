using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Veritabani_Odevi.Data;
using Veritabani_Odevi.Filters;
using Veritabani_Odevi.Models;
using Veritabani_Odevi.Models.ViewModels;

namespace Veritabani_Odevi.Controllers
{
    public class KullaniciController : Controller
    {
        private readonly KullaniciRepository _kullaniciRepo;
        public KullaniciController(KullaniciRepository _kullaniciRepository)
        {
            _kullaniciRepo = _kullaniciRepository;
        }

        [HttpGet]
        public IActionResult GirisKayit()
        {
            // Varsayılan olarak Giriş sekmesi aktif
            var viewModel = new GirisKayitViewModel { AktifSekme = "Giris" };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // GirisViewModel al
        public async Task<IActionResult> Giris(GirisViewModel model, string returnUrl = null)
        {
            // GirisViewModel için doğrulama yap
            if (TryValidateModel(model, nameof(model)))
            {
                // Doğrulama ve kullanıcı arama için 'model' nesnesini kullan (bu zaten GirisViewModel)
                var kullanici = _kullaniciRepo.KullaniciyiGetirByEmail(model.Email);
                if (kullanici != null && VerifyPassword(model.Password, kullanici.SifreHash))
                {
                    // Basit oturum yönetimi
                    HttpContext.Session.SetInt32("KullaniciID", kullanici.KullaniciID);
                    HttpContext.Session.SetString("KullaniciAdi", $"{kullanici.Ad} {kullanici.Soyad}");
                    HttpContext.Session.SetString("KullaniciRol", kullanici.Rol);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Hata mesajını genel hata olarak ekle
                    ModelState.AddModelError("", "E-posta veya şifre hatalı.");
                }
            }

            // Hata varsa, Giriş sekmesi açık kalmalı
            // GirisKayitViewModel oluştur ve Giris property'sine modeli koy
            var viewModel = new GirisKayitViewModel
            {
                Giris = model,
                AktifSekme = "Giris" // Hata durumunda giriş sekmesi aktif
            };
            return View("GirisKayit", viewModel); // GirisKayitViewModel ile geri gönder
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kayit(KayitViewModel model)
        {
            if (TryValidateModel(model, nameof(model)))
            {
                if (_kullaniciRepo.KullaniciyiGetirByEmail(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
                    var errorViewModel = new GirisKayitViewModel // <-- Yeni isim
                    {
                        Kayit = model,
                        AktifSekme = "Kayit"
                    };
                    return View("GirisKayit", errorViewModel);
                }

                try
                {
                    var yeniKullanici = new Kullanici
                    {
                        Ad = model.FullName.Split(' ').First(),
                        Soyad = string.Join(" ", model.FullName.Split(' ').Skip(1)),
                        Eposta = model.Email,
                        SifreHash = HashPassword(model.Password),
                        Telefon = "",
                        Adres = "",
                        KayitTarihi = DateTime.Now,
                        Derecelendirme = 0,
                        Rol = "Uye"
                    };

                    int yeniID = _kullaniciRepo.KullaniciEkle(yeniKullanici);
                    TempData["Mesaj"] = "Kayıt başarılı! Artık giriş yapabilirsiniz.";

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    // Hata durumunda da yeni bir ViewModel oluştur
                    var errorViewModel = new GirisKayitViewModel // <-- Yeni isim
                    {
                        Kayit = model,
                        AktifSekme = "Kayit"
                    };
                    return View("GirisKayit", errorViewModel);
                }
            }

            // TryValidateModel başarısız olursa (ModelState.IsValid false)
            var invalidModelViewModel = new GirisKayitViewModel // <-- Yeni isim
            {
                Kayit = model,
                AktifSekme = "Kayit"
            };
            return View("GirisKayit", invalidModelViewModel);
        }
        // GET: Kullanici/TumKullanicilar
        [AdminYetki]
        public IActionResult TumKullanicilar()
        {
            var tumKullanicilar = _kullaniciRepo.TumKullanicilariGetir();
            return View(tumKullanicilar); // Views/Kullanici/TumKullanicilar.cshtml görünümüne verileri gönderir
        }

        // GET: Kullanici/Detay/5
        public IActionResult Detay(int id)
        {
            var kullanici = _kullaniciRepo.KullaniciyiGetir(id);
            if (kullanici == null)
            {
                return NotFound(); // Kullanıcı yoksa 404 döndür
            }
            return View(kullanici); // Views/Kullanici/Detay.cshtml görünümüne kullanıcıyı gönderir
        }

        // GET: Kullanici/Duzenle/5
        [AdminYetki]
        public IActionResult Duzenle(int id)
        {
            var kullanici = _kullaniciRepo.KullaniciyiGetir(id);
            if (kullanici == null)
            {
                return NotFound(); // Kullanıcı yoksa 404 döndür
            }
            return View(kullanici); // Views/Kullanici/Duzenle.cshtml görünümüne kullanıcıyı gönderir
        }

        // POST: Kullanici/Duzenle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminYetki]
        public IActionResult Duzenle(int id, Kullanici model)
        {
            // Gelen id ile modelin KullaniciID'sinin eşleşip eşleşmediğini kontrol et
            // Bu, bir kullanıcının başka bir kullanıcının ID'siyle istek göndermesini engeller
            if (id != model.KullaniciID)
            {
                return NotFound(); // ID'ler uyuşmuyorsa 404 döndür
            }

            // SifreHash modelstate'ten çıkar (form'dan gelmediği için)
            ModelState.Remove("SifreHash");

            if (ModelState.IsValid)
            {
                // Mevcut kullanıcının verilerini al
                var mevcutKullanici = _kullaniciRepo.KullaniciyiGetir(id);
                if (mevcutKullanici == null)
                {
                    return NotFound();
                }

                // SifreHash'i eski değer ile koru (form'dan gelmediği için)
                model.SifreHash = mevcutKullanici.SifreHash;
                model.KayitTarihi = mevcutKullanici.KayitTarihi; // KayitTarihi'ni de koru

                if (_kullaniciRepo.KullaniciGuncelle(model))
                {
                    TempData["Mesaj"] = "Kullanıcı başarıyla güncellendi.";
                    return RedirectToAction(nameof(TumKullanicilar)); // veya Detay(id) action'ına yönlendir
                }
                else
                {
                    // Güncelleme başarısız olursa genel bir hata mesajı ekle
                    ModelState.AddModelError("", "Kullanıcı güncellenemedi. Lütfen tekrar deneyin.");
                }
            }
            // ModelState geçerli değilse veya güncelleme başarısızsa, formu tekrar göster
            return View(model);
        }



        // GET: Kullanici/Sil/5
        [AdminYetki]
        public IActionResult Sil(int id)
        {
            var kullanici = _kullaniciRepo.KullaniciyiGetir(id);
            if (kullanici == null)
            {
                return NotFound(); // Kullanıcı yoksa 404 döndür
            }
            return View(kullanici); // Views/Kullanici/Sil.cshtml görünümüne kullanıcıyı gönderir (onay için)
        }

        // POST: Kullanici/Sil/5
        [HttpPost, ActionName("Sil")] // ActionName attribute'u, bu metodun "Sil" URL'sine yanıt vereceğini belirtir
        [ValidateAntiForgeryToken]
        [AdminYetki]
        public IActionResult SilOnay(int id)
        {
            try
            {
                if (_kullaniciRepo.KullaniciSil(id))
                {
                    TempData["Mesaj"] = "Kullanıcı başarıyla silindi.";
                }
                else
                {
                    TempData["Hata"] = "Kullanıcı silinemedi.";
                }
            }
            catch (Exception ex)
            {
                TempData["Hata"] = ex.Message; // Repository'den gelen özel hata mesajı
            }
            return RedirectToAction(nameof(TumKullanicilar)); // Silme işleminden sonra listeye dön
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cikis()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // Şifre hashleme fonksiyonu
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        // Şifre doğrulama fonksiyonu
        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        // ... diğer action'lar (Duzenle, Sil, Cikis) ...
    }

}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Veritabani_Odevi.Filters
{
    /// <summary>
    /// Sadece Admin rolündeki kullanıcıların erişebileceği action'lar için attribute
    /// </summary>
    public class AdminYetkiAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var kullaniciRol = context.HttpContext.Session.GetString("KullaniciRol");
            
            if (string.IsNullOrEmpty(kullaniciRol) || kullaniciRol != "Admin")
            {
                // Admin değilse, erişim engellendi sayfasına veya anasayfaya yönlendir
                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }
            
            base.OnActionExecuting(context);
        }
    }
    
    /// <summary>
    /// Giriş yapmış kullanıcı gerekli action'lar için attribute
    /// </summary>
    public class GirisGerekliAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var kullaniciID = context.HttpContext.Session.GetInt32("KullaniciID");
            
            if (!kullaniciID.HasValue)
            {
                // Giriş yapmamışsa, giriş sayfasına yönlendir
                context.Result = new RedirectToActionResult("GirisKayit", "Kullanici", null);
                return;
            }
            
            base.OnActionExecuting(context);
        }
    }
}

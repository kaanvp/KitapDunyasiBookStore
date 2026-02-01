namespace Veritabani_Odevi.Models.ViewModels
{
    public class GirisKayitViewModel
    {
        public GirisViewModel Giris { get; set; } = new GirisViewModel();
        public KayitViewModel Kayit { get; set; } = new KayitViewModel();

        // Hangi sekme aktif olacak (Giris / Kayit)
        public string AktifSekme { get; set; } = "Giris";
    }
}

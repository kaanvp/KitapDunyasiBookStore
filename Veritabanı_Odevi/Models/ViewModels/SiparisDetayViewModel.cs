namespace Veritabani_Odevi.Models.ViewModels
{
    public class SiparisDetayViewModel
    {
        public string KitapAdi { get; set; }
        public int Adet { get; set; }
        public decimal Fiyat { get; set; }

        public decimal Tutar => Adet * Fiyat;
        public decimal AltToplam => Adet * Fiyat;
        // ✅ EKLE
        public string KapakResmiUrl { get; set; }
    }
}

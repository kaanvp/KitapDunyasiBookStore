namespace Veritabani_Odevi.Models.ViewModels
{
    public class KitapDetayViewModel
    {
        public int KitapID { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string? Yazar { get; set; }
        public decimal Fiyat { get; set; }
        public decimal GecerliFiyat { get; set; }
        public string SaticiAdSoyad { get; set; } = string.Empty;
        public decimal SaticiPuan { get; set; }
        public string Kategoriler { get; set; } = string.Empty;
        public decimal OrtalamaYorumPuan { get; set; }
        public int YorumSayisi { get; set; }
        public string? IndirimAdi { get; set; }
        public decimal? IndirimOrani { get; set; }
    }
}

namespace Veritabani_Odevi.DTOs
{
    public class KitapListeDTO
    {
        public int KitapID { get; set; }
        public string Baslik { get; set; }
        public string Yazar { get; set; }
        public decimal OrijinalFiyat { get; set; }
        public decimal? IndirimOrani { get; set; }
        public decimal IndirimliFiyat { get; set; }
        public string KapakResmiUrl { get; set; }
        public int StokAdedi { get; set; }
        public int SaticiKullaniciID { get; set; }
        public string Durum { get; set; }
        
        public bool IndirimVar => IndirimOrani.HasValue && IndirimOrani.Value > 0;
    }
}

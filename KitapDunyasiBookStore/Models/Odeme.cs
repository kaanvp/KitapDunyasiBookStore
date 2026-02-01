namespace Veritabani_Odevi.Models
{
    public class Odeme
    {
        public int OdemeID { get; set; }
        public int SiparisID { get; set; }
        public string OdemeTuru { get; set; } // KrediKarti / Havale / KapidaOdeme
        public DateTime IslemTarihi { get; set; }
        public decimal Tutar { get; set; }
        public string BankaOnayKodu { get; set; }
        public string Durum { get; set; } // Basarili / Basarisiz / Beklemede
    }
}

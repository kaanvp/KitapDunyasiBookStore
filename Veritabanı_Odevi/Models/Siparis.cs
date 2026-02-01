namespace Veritabani_Odevi.Models
{
    public class Siparis
    {
        public int SiparisID { get; set; }
        public string SiparisNumarasi { get; set; }
        public int KullaniciID { get; set; }
        public DateTime SiparisTarihi { get; set; }
        public decimal ToplamTutar { get; set; }
        public string TeslimatAdresi { get; set; }
        public string Durum { get; set; } // Hazırlanıyor, Kargoda, TeslimEdildi, Iptal

        // İlişkili veriler
        public List<SiparisDetay> Detaylar { get; set; } = new List<SiparisDetay>();
        public string KullaniciAdSoyad { get; set; } // JOIN ile gelecek
    }
}

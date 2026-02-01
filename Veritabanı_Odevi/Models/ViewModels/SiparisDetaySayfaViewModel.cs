namespace Veritabani_Odevi.Models.ViewModels
{
    public class SiparisDetaySayfaViewModel
    {
        // ÜST BİLGİLER (Siparis tablosu)
        public string SiparisNo { get; set; }
        public DateTime SiparisTarihi { get; set; }
        public string Durum { get; set; }
        public string TeslimatAdresi { get; set; }
        public decimal ToplamTutar { get; set; }

        // 👇 ÖDEME BİLGİLERİ
        public string? OdemeTuru { get; set; }
        public string? OdemeDurum { get; set; }
        public DateTime? OdemeTarihi { get; set; }
        public string? BankaOnayKodu { get; set; }

        public List<SiparisDetayViewModel> Urunler { get; set; }
    }
}

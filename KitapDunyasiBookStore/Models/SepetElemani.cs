namespace Veritabani_Odevi.Models
{
    public class SepetElemani
    {
        public int SepetID { get; set; }
        public int KullaniciID { get; set; }
        public int KitapID { get; set; }
        public int Adet { get; set; }
        public DateTime EklenmeTarihi { get; set; }

        // İndirim bilgileri
        public decimal OrijinalFiyat { get; set; }
        public decimal IndirimliFiyat { get; set; }
        public decimal? IndirimOrani { get; set; }

        // İlişkili veriler
        public Kitap kitap { get; set; }
    }
}


namespace Veritabani_Odevi.Models
{
    public class Yorum
    {
        public int YorumID { get; set; }
        public int KitapID { get; set; }
        public int KullaniciID { get; set; }
        public string Baslik { get; set; }
        public string Icerik { get; set; }
        public int Puan { get; set; } // 1-5 arası
        public DateTime YorumTarihi { get; set; }
        public bool OnayDurumu { get; set; }

        // İlişkili veriler
        public string KullaniciAdSoyad { get; set; } // JOIN ile gelecek
    }
}

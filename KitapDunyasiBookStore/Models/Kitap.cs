namespace Veritabani_Odevi.Models
{
    public class Kitap
    {
        public int KitapID { get; set; }          // PK
        public string Baslik { get; set; }        // NOT NULL
        public string Aciklama { get; set; }      // TEXT
        public string Yazar { get; set; }         // VARCHAR(150)
        public string YayinEvi { get; set; }      // VARCHAR(100)
        public string ISBN { get; set; }          // UNIQUE
        public decimal Fiyat { get; set; }        // DECIMAL(10,2)
        public int StokAdedi { get; set; }        // DEFAULT 0
        public DateTime EklenmeTarihi { get; set; } // DEFAULT CURRENT_TIMESTAMP
        public DurumEnum Durum { get; set; }      // ENUM
        public string KapakResmiUrl { get; set; } // VARCHAR(500)
        public int SaticiKullaniciID { get; set; } // FK -> Kullanici.KullaniciID
    }
    public enum DurumEnum
    {
        Yeni,
        CokIyi,
        Iyi,
        Orta
    }
}

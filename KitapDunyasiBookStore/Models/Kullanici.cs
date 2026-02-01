namespace Veritabani_Odevi.Models
{
    public class Kullanici
    {
        public int KullaniciID { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string Eposta { get; set; }
        public string SifreHash { get; set; }
        public string Telefon { get; set; }
        public string Adres { get; set; }
        public DateTime KayitTarihi { get; set; }
        public decimal Derecelendirme { get; set; }
        public string Rol { get; set; } // Admin veya Uye
    }
}

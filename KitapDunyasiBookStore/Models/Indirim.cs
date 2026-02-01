namespace Veritabani_Odevi.Models
{
    public class Indirim
    {
        public int IndirimID { get; set; }
        public string IndirimAdi { get; set; }
        public decimal IndirimOrani { get; set; } // % indirim (örn: 10.00 = %10)
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public bool Aktif { get; set; }

        // İlişkili veriler
        public List<Kitap> Kitaplar { get; set; } = new List<Kitap>();
    }
}

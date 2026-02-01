namespace Veritabani_Odevi.Models
{
    public class SiparisDetay
    {
        public int SiparisDetayID { get; set; }
        public int SiparisID { get; set; }
        public int KitapID { get; set; }
        public int Adet { get; set; }
        public decimal BirimFiyat { get; set; }

        // İlişkili veriler
        public string KitapBaslik { get; set; } // JOIN ile gelecek
    }
}

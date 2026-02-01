namespace Veritabani_Odevi.Models
{
    public class KitapIndirim
    {
        public int KitapID { get; set; }
        public int IndirimID { get; set; }

        // İlişkili veriler
        public string KitapBaslik { get; set; } // JOIN ile gelecek
        public string IndirimAdi { get; set; }  // JOIN ile gelecek
        public decimal IndirimOrani { get; set; } // JOIN ile gelecek
    }
}

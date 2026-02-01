namespace Veritabani_Odevi.Models.ViewModels
{
    public class SepetElemaniViewModel
    {
        public int SepetElemaniID { get; set; }
        public int KitapID { get; set; }
        public string Baslik { get; set; }
        public decimal OrijinalFiyat { get; set; }
        public decimal IndirimliFiyat { get; set; }
        public decimal? IndirimOrani { get; set; }
        public bool IndirimVar => IndirimOrani.HasValue && IndirimOrani.Value > 0;
        public int Adet { get; set; }
        public string KapakResmiUrl { get; set; }
    }
}

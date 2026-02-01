namespace Veritabani_Odevi.DTOs
{
    public class IndirimdekiKitapDTO
    {
        public int KitapID { get; set; }
        public string Baslik { get; set; }
        public string Yazar { get; set; }
        public decimal OrijinalFiyat { get; set; }
        public decimal IndirimOrani { get; set; }
        public decimal IndirimliFiyat { get; set; }
        public decimal IndirilenTutar { get; set; }
        public string IndirimAdi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public string Kategoriler { get; set; }
        public string SaticiAdSoyad { get; set; }
        public int StokAdedi { get; set; }
        public string KapakResmiUrl { get; set; }
    }
}

namespace Veritabani_Odevi.DTOs
{
    public class SiparisDetayDTO
    {
        public string SiparisNumarası { get; set; }
        public DateTime SiparisTarihi { get; set; }
        public decimal ToplamTutar { get; set; }
        public string SiparisDurumu { get; set; }
        public string TeslimatAdresi { get; set; }
        public string KitapBaslik { get; set; }
        public string Yazar { get; set; }
        public int Adet { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal AltToplam { get; set; }
        public string SaticiAdSoyad { get; set; }
        public string SaticiEposta { get; set; }
        public string SaticiTelefon { get; set; }
        public string KapakResmiUrl { get; set; }
    }
}

namespace Veritabani_Odevi.DTOs
{
    public class KategoriYorumIstatistik
    {
        public string KategoriAdi { get; set; } = string.Empty;
        public int ToplamKitapSayisi { get; set; }
        public int ToplamYorumSayisi { get; set; }
        public decimal OrtalamaPuan { get; set; }
        public int BesYildizSayisi { get; set; }
        public int DortArtıYildizSayisi { get; set; }
        public int FarkliYorumcuSayisi { get; set; }
    }
}
